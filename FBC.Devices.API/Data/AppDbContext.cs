using FBC.Devices.API.Data.Repositories;
using FBC.Devices.API.Models;
using FBC.Devices.API.Services;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Data;

public class AppDbContext : DbContext
{
    public DbSet<AddrType> AddrTypes { get; set; }
    public DbSet<DeviceType> DeviceTypes { get; set; }
    public DbSet<DeviceGroup> DeviceGroups { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceAddr> DeviceAddresses { get; set; }
    public DbSet<AppUser> SysUsers { get; set; }
    public DbSet<DeviceSearchData> DeviceSearchMetas { get; set; }

    public string DbPath { get; }

    public AppDbContext()
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
        DbPath = Path.Combine(dir, "FBC.Devices.db");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
        DbPath = Path.Combine(dir, "FBC.Devices.db");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    public static void MigrateDB()
    {
        using var db = new AppDbContext();
        try
        {
            Console.WriteLine("Begin Migrate");

            // If this is a DB copied from the old Blazor project,
            // mark our InitialCreate migration as applied since schema is identical
            if (db.Database.CanConnect())
            {
                var applied = db.Database.GetAppliedMigrations().ToList();
                var pending = db.Database.GetPendingMigrations().ToList();
                if (applied.Any() && pending.Contains("20260321154003_InitialCreate"))
                {
                    Console.WriteLine("Legacy DB detected — marking InitialCreate as applied");
                    db.Database.ExecuteSqlRaw(
                        "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20260321154003_InitialCreate', '9.0.9')");
                }
            }

            db.Database.Migrate();
            Console.WriteLine("End Migrate");

            if (!db.SysUsers.Any(x => x.IsSysAdmin))
            {
                Console.WriteLine("Creating SysAdmin User with default credentials (username: admin, password: admin)");
                Console.WriteLine("Please change the password after first login!");
                var user = new AppUser
                {
                    UserName = "admin",
                    Password = Constants.Tools.ToMD5("admin"),
                    IsSysAdmin = true,
                    Roles = Constants.UserRoles.SysAdmin,
                    Name = "System Administrator"
                };
                var repo = new UserRepository(db);
                repo.ApplyOperation(DBRepository.EntityOperation.Create, user, true).GetAwaiter().GetResult();
            }

            if (!db.AddrTypes.Any())
            {
                Console.WriteLine("Creating Connection Types");
                db.AddrTypes.AddRange(
                    new AddrType { Name = "TCP/IP" },
                    new AddrType { Name = "HTTP" },
                    new AddrType { Name = "RTSP" },
                    new AddrType { Name = "FTP" },
                    new AddrType { Name = "SSH" },
                    new AddrType { Name = "Telnet" },
                    new AddrType { Name = "SDK" },
                    new AddrType { Name = "MAC Address" }
                );
                db.SaveChanges();
            }

            if (!db.DeviceTypes.Any())
            {
                Console.WriteLine("Creating Device Types");
                db.DeviceTypes.AddRange(
                    new DeviceType { Name = "Camera" },
                    new DeviceType { Name = "NVR" },
                    new DeviceType { Name = "DVR" },
                    new DeviceType { Name = "Switch" },
                    new DeviceType { Name = "Router" },
                    new DeviceType { Name = "VM" },
                    new DeviceType { Name = "Server" },
                    new DeviceType { Name = "PC" },
                    new DeviceType { Name = "Laptop" },
                    new DeviceType { Name = "Tablet" },
                    new DeviceType { Name = "Phone" },
                    new DeviceType { Name = "Printer" },
                    new DeviceType { Name = "Scanner" },
                    new DeviceType { Name = "Firewall" }
                );
                db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Migrate: " + ex.Message);
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseSqlite($"Data Source={DbPath}");
            options.EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map Entity<TId,TEntity>.Id to existing column names
        modelBuilder.Entity<Device>().Property(e => e.Id).HasColumnName("DeviceId");
        modelBuilder.Entity<DeviceAddr>().Property(e => e.Id).HasColumnName("DeviceAddrId");
        modelBuilder.Entity<DeviceGroup>().Property(e => e.Id).HasColumnName("DeviceGroupId");
        modelBuilder.Entity<DeviceType>().Property(e => e.Id).HasColumnName("DeviceTypeId");
        modelBuilder.Entity<AddrType>().Property(e => e.Id).HasColumnName("AddrTypeId");
        modelBuilder.Entity<AppUser>().ToTable("SysUsers").Property(e => e.Id).HasColumnName("UserId");

        // Foreign key relationships
        modelBuilder.Entity<Device>()
            .HasOne(d => d.DeviceGroup)
            .WithMany()
            .HasForeignKey(d => d.DeviceGroupId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Device>()
            .HasOne(d => d.DeviceType)
            .WithMany()
            .HasForeignKey(d => d.DeviceTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DeviceAddr>()
            .HasOne(d => d.AddrType)
            .WithMany()
            .HasForeignKey(d => d.AddrTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        base.OnModelCreating(modelBuilder);
    }

    #region Change tracking for search service

    private static readonly HashSet<Type> WatchedTypes = new()
    {
        typeof(Device), typeof(DeviceAddr), typeof(DeviceGroup),
        typeof(DeviceType), typeof(AddrType)
    };

    private bool RelevantChangeDetected() =>
        ChangeTracker.Entries()
            .Any(e => (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                && WatchedTypes.Contains(e.Entity.GetType()));

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var relevantChange = RelevantChangeDetected();
        var rows = base.SaveChanges(acceptAllChangesOnSuccess);
        if (relevantChange && rows > 0)
            DeviceSearchDataService.RequestImmediateScan();
        return rows;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var relevantChange = RelevantChangeDetected();
        var rows = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        if (relevantChange && rows > 0)
            DeviceSearchDataService.RequestImmediateScan();
        return rows;
    }

    #endregion

    // Backup functionality moved to Features/Backup/ using SQLite Online Backup API
}
