using FBC.Devices.API.Data;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Services;

public class DeviceSearchDataService : BackgroundService
{
    private readonly ILogger<DeviceSearchDataService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private static volatile bool _immediateScanRequested;
    private CancellationTokenSource? _immediateRequestStopTokenSource;

    public static void RequestImmediateScan() => _immediateScanRequested = true;

    public DeviceSearchDataService(ILogger<DeviceSearchDataService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        _immediateRequestStopTokenSource = new CancellationTokenSource();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (await DoWork(stoppingToken))
            {
                if (_immediateScanRequested)
                {
                    _immediateScanRequested = false;
                    _logger.LogInformation("Immediate scan requested, starting next cycle immediately.");
                    _immediateRequestStopTokenSource.Cancel();
                    _immediateRequestStopTokenSource?.Dispose();
                    _immediateRequestStopTokenSource = new CancellationTokenSource();
                    continue;
                }

                try
                {
                    using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                        stoppingToken, _immediateRequestStopTokenSource.Token);
                    await Task.Delay(TimeSpan.FromMinutes(3), linked.Token);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Delay cancelled.");
                }
            }
            else
            {
                _logger.LogInformation("Previous work still in progress, skipping this cycle.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("DeviceSearchDataService is stopping.");
    }

    private readonly SemaphoreSlim _workLock = new(1, 1);

    private async Task<bool> DoWork(CancellationToken stoppingToken)
    {
        if (!await _workLock.WaitAsync(0, stoppingToken)) return false;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var allDeviceIds = await db.Devices.AsNoTracking()
                .Select(d => d.Id).ToListAsync(stoppingToken);

            foreach (var devicePk in allDeviceIds)
                await DeviceSearchDataHelper.SyncDeviceSearchData(db, devicePk, _logger, stoppingToken);

            var orphaned = await db.DeviceSearchMetas
                .Where(d => !db.Devices.Any(dev => dev.Id == d.DeviceId))
                .ToListAsync(stoppingToken);

            if (orphaned.Any())
            {
                _logger.LogInformation("Deleting {Count} orphaned search data entries.", orphaned.Count);
                db.DeviceSearchMetas.RemoveRange(orphaned);
                await db.SaveChangesAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DoWork");
        }
        finally
        {
            _workLock.Release();
        }
        return true;
    }
}
