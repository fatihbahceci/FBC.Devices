using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.DBModels
{
    public class DB : DbContext
    {
        //public DbSet<SysUser> Users { get; set; }
        public DbSet<AddrType> AddrTypes { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<DeviceGroup> DeviceGroups { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceAddr> DeviceAddresses { get; set; }
        public DbSet<DBUser> SysUsers { get; set; }


        public string DbPath { get; }

        public DB()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
            DbPath = Path.Combine(dir, "FBC.Devices.db");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public static void MigrateDB()
        {
            using (var db = new DB())
            {
                try
                {
                    Console.WriteLine("Begin Migrate");

                    db.Database.Migrate();
                    Console.WriteLine("End Migrate");

                    if (db.SysUsers.Any(x => x.IsSysAdmin) == false)
                    {
                        Console.WriteLine("Creating SysAdmin User with default credentials (username: admin, password: admin)");
                        Console.WriteLine("Please change the password after first login!");
                        var user = new DBUser()
                        {
                            UserName = "admin",
                            Password = C.Tools.ToMD5("admin"),
                            IsSysAdmin = true,
                            Roles = C.UserRoles.SysAdmin,
                            Name = "System Administrator"
                        };
                        user.AdjustData(true);
                        db.SysUsers.Add(user);
                        db.SaveChanges();
                    }

                    if (!db.AddrTypes.Any())
                    {
                        Console.WriteLine("Creating Connection Types");
                        db.AddrTypes.AddRange(new AddrType[]
                        {
                            new AddrType() { Name = "TCP/IP" },
                            new AddrType() { Name = "HTTP" },
                            new AddrType() { Name = "RTSP" },
                            new AddrType() { Name = "FTP" },
                            new AddrType() { Name = "SSH" },
                            new AddrType() { Name = "Telnet" },
                            new AddrType() { Name = "SDK" },
                            new AddrType() { Name = "MAC Address" },
                        });
                        db.SaveChanges();
                    }
                    if (!db.DeviceTypes.Any())
                    {
                        Console.WriteLine("Creating Device Types");
                        db.DeviceTypes.AddRange(new DeviceType[]
                        {
                            new DeviceType() { Name = "Camera" },
                            new DeviceType() { Name = "NVR" },
                            new DeviceType() { Name = "DVR" },
                            new DeviceType() { Name = "Switch" },
                            new DeviceType() { Name = "Router" },
                            new DeviceType() { Name = "VM" },
                            new DeviceType() { Name = "Server" },
                            new DeviceType() { Name = "PC" },
                            new DeviceType() { Name = "Laptop" },
                            new DeviceType() { Name = "Tablet" },
                            new DeviceType() { Name = "Phone" },
                            new DeviceType() { Name = "Printer" },
                            new DeviceType() { Name = "Scanner" },
                            new DeviceType() { Name = "Firewall" },
                        });
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Migrate: " + ex.Message);
                    //Console.WriteLine(ex.StackTrace);
                }
                finally
                {

                }
            }
        }


        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
            options.EnableSensitiveDataLogging(); // Enable sensitive data logging for debugging purposes
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<DovizKuru>()
            //    .HasIndex(d => d.DovizCinsi)
            //    .IsUnique();

            //modelBuilder.Entity<DovizKuruHareket>()
            //    .HasIndex(d => new { d.Tarih, d.DovizKuruId })
            //    .IsUnique();

            // DeviceGroup Foreign Key
            modelBuilder.Entity<Device>()
                .HasOne(d => d.DeviceGroup)
                .WithMany()
                .HasForeignKey(d => d.DeviceGroupId)
                .OnDelete(DeleteBehavior.NoAction);

            // DeviceType Foreign Key
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
            //modelBuilder.Entity<DeviceAddr>()
            //    .HasOne(d => d.Device)
            //    .WithMany()
            //    .HasForeignKey(d => d.DeviceId)
            //    .OnDelete(DeleteBehavior.NoAction);

            //        modelBuilder.Entity<DeviceAddr>()
            //.HasOne(x => x.Device)
            //.WithMany(x => x.DeviceAddresses)
            //.OnDelete(DeleteBehavior.ClientNoAction);

            base.OnModelCreating(modelBuilder);
        }
        #region Backup
        public string SaveChangesAndBackup()
        {
            SaveChanges();

            ExecuteCheckpointAndClose();
            return CreateBackup();
        }
        private void ExecuteCheckpointAndClose()
        {
            using (var connection = Database.GetDbConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    // Set the database to single user mode
                    //Sanıyorum ki .db-shm .db-val dosyalarını bu siliyor.
                    command.CommandText = "PRAGMA journal_mode = DELETE;";
                    command.ExecuteNonQuery();

                    // Perform a checkpoint to ensure all changes are written
                    command.CommandText = "PRAGMA wal_checkpoint(FULL);";
                    command.ExecuteNonQuery();

                    command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                    command.ExecuteNonQuery();

                    command.CommandText = "VACUUM;";
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            // Ensure all finalizers have run and resources are released
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private string CreateBackup()
        {
            string backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DatabaseBackups");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            string backupPath = Path.Combine(backupDir, $"FBC.Devices_backup_{DateTime.Now:yyyyMMddHHmmss}.db");
            File.Copy(DbPath, backupPath, true);
            return backupPath;
        }
        #endregion
    }
}