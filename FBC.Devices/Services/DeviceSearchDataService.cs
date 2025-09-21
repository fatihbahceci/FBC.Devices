using FBC.Devices.DBModels;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.Services;


public class DeviceSearchDataService : BackgroundService, IHostedService
{
    private readonly ILogger logger;
    //Altrnatively, use a DbContextFactory: private readonly IDbContextFactory<DB> _dbFactory;

    public DeviceSearchDataService(ILogger<DeviceSearchDataService> logger/*, IDbContextFactory<DB> dbFactory*/)
    {
        this.logger = logger;
        //this._dbFactory = dbFactory;
    }
    //https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/background-tasks-with-ihostedservice
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // startup delay
        while (!stoppingToken.IsCancellationRequested)
        {
            if (await DoWork(stoppingToken))
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            else
            {
                logger.LogInformation("Previous work still in progress, skipping this cycle.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
        logger.LogInformation("DeviceSearchDataService is stopping.");
        await Task.CompletedTask;
    }
    private readonly SemaphoreSlim _workLock = new SemaphoreSlim(1, 1);
   
    private async Task<bool> DoWork(CancellationToken stoppingToken)
    {
        if (!await _workLock.WaitAsync(0, stoppingToken)) return false;
        try
        {
            using var db = new DB();
            //Alternatively, await using var db = await _dbFactory.CreateDbContextAsync(ct);
            var allDeviceIds = await db.Devices.AsNoTracking().Select(d => d.DeviceId).ToListAsync(stoppingToken);
            foreach (var devicePk in allDeviceIds)
            {
                await DeviceSearchDataHelper.SyncDeviceSearchData(db, devicePk, logger, stoppingToken);
            }

            //Delete orphaned search data (if any)
            var orphaned = await db.DeviceSearchMetas
                .Where(d => !db.Devices.Any(dev => dev.DeviceId == d.DeviceId))
                .ToListAsync(stoppingToken);
            if (orphaned.Any())
            {
                logger.LogInformation($"Deleting {orphaned.Count} orphaned search data entries.");
                db.DeviceSearchMetas.RemoveRange(orphaned);
                await db.SaveChangesAsync(stoppingToken);
            }
            // Or
            // var entityType = db.Model.FindEntityType(typeof(DeviceSearchData));
            // var tableName = entityType.GetTableName();
            // await db.Database.ExecuteSqlRawAsync($"DELETE FROM {tableName} WHERE DeviceId NOT IN (SELECT DeviceId FROM Devices)", stoppingToken);
            // await db.Database.ExecuteSqlRawAsync(@"DELETE FROM DeviceSearchData WHERE DeviceId NOT IN (SELECT DeviceId FROM Devices)", stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in DoWork:" + ex.Message + Environment.NewLine + ex.StackTrace);
        }
        finally
        {
            _workLock.Release();
        }
        return true;
    }
}
