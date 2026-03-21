using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using FBC.Devices.API.Data;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Services;

public class DeviceAddressStatus
{
    public DateTime LastPingTime { get; set; }
    public DateTime LastSuccessPingTime { get; set; }
    public bool IsSuccess => !IsDefault && LastSuccessPingTime >= LastPingTime;
    public bool IsDefault => LastPingTime == DateTime.MinValue && LastSuccessPingTime == DateTime.MinValue;
}

public class DeviceStatusService : BackgroundService
{
    private readonly ILogger<DeviceStatusService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<int, DeviceAddressStatus> _statuses = new();

    public DeviceStatusService(ILogger<DeviceStatusService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public static DeviceAddressStatus? GetDeviceAddressStatus(int deviceAddrId)
    {
        _statuses.TryGetValue(deviceAddrId, out var status);
        return status;
    }

    public static IReadOnlyDictionary<int, DeviceAddressStatus> GetAllStatuses() => _statuses;

    private static void AddOrUpdateStatus(int deviceAddrId, bool isSuccess)
    {
        var existing = GetDeviceAddressStatus(deviceAddrId);
        var now = DateTime.Now;
        _statuses[deviceAddrId] = new DeviceAddressStatus
        {
            LastPingTime = now,
            LastSuccessPingTime = isSuccess ? now : (existing?.LastSuccessPingTime ?? DateTime.MinValue)
        };
    }

    private bool? _executeStatus;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            switch (_executeStatus)
            {
                case null:
                    _logger.LogWarning("First work skipping for the site's startup");
                    _executeStatus = false;
                    await Task.Delay(10000, stoppingToken);
                    break;
                case false:
                    _executeStatus = true;
                    _logger.LogWarning("This is startup work.");
                    await SendPingToAllAddresses(stoppingToken);
                    _logger.LogWarning("Startup work has been ended.");
                    break;
                case true:
                    await SendPingToAllAddresses(stoppingToken);
                    break;
            }
        }
    }

    private bool _isBusy;
    private async Task SendPingToAllAddresses(CancellationToken stoppingToken)
    {
        if (_isBusy) return;
        _isBusy = true;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var addresses = await db.Devices.AsNoTracking()
                .Where(x => x.IsActive)
                .SelectMany(x => x.DeviceAddresses)
                .Where(x => x.PeriodicPingCheck)
                .ToListAsync(stoppingToken);

            foreach (var addr in addresses)
            {
                try
                {
                    _logger.LogInformation("Ping to {Addr}", addr.Addr);
                    var ping = new Ping();
                    var reply = ping.Send(addr.Addr ?? "", 5000);
                    _logger.LogInformation("Ping to {Addr} is {Status}", addr.Addr, reply.Status);
                    AddOrUpdateStatus(addr.Id, reply.Status == IPStatus.Success);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pinging {Addr}", addr.Addr);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendPingToAllAddresses");
        }
        finally
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (TaskCanceledException) { }
            _isBusy = false;
        }
    }
}
