
using Microsoft.EntityFrameworkCore;
using FBC.Devices.DBModels;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;
namespace FBC.Devices.Services;

public class DeviceAddressStatus
{
    //public int DeviceAddrId { get; set; }
    public DateTime LastPingTime { get; set; }
    public DateTime LastSuccessPingTime { get; set; }

    public bool IsSuccess => !IsDefault && LastSuccessPingTime >= LastPingTime;

    public bool IsDefault => LastPingTime == DateTime.MinValue && LastSuccessPingTime == DateTime.MinValue;

    public static DeviceAddressStatus GetDeviceAddressStatusOrDefault(int deviceAddrId)
    {
        return DeviceStatusService.GetDeviceAddressStatus(deviceAddrId) ?? new DeviceAddressStatus()
        {
            LastPingTime = DateTime.MinValue,
            LastSuccessPingTime = DateTime.MinValue
        };
    }
}
public class DeviceStatusService : BackgroundService, IHostedService
{
    private ILogger logger;
    private static ConcurrentDictionary<int, DeviceAddressStatus> deviceAddressStatuses = new ConcurrentDictionary<int, DeviceAddressStatus>();

    private static void AddOrUpdateDeviceAddressStatus(int deviceAddrId, bool isSuccess)
    {
        var existing = GetDeviceAddressStatus(deviceAddrId);
        DateTime now = DateTime.Now;
        deviceAddressStatuses[deviceAddrId] = new DeviceAddressStatus
        {
            LastPingTime = now,
            LastSuccessPingTime = isSuccess ? now : (existing != null ? existing.LastSuccessPingTime : DateTime.MinValue)
        };
    }
    public static DeviceAddressStatus? GetDeviceAddressStatus(int deviceAddrId)
    {
        if (deviceAddressStatuses.TryGetValue(deviceAddrId, out var status))
        {
            return status;
        }
        else
        {
            return null;
        }

    }

    public DeviceStatusService(ILogger<DeviceStatusService> logger)
    {
        this.logger = logger;
    }
    //https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/background-tasks-with-ihostedservice
    bool? executeStatus = null;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            switch (executeStatus)
            {
                case null:
                    logger.LogWarning("First work skipping for the site's startup");
                    executeStatus = false;
                    await Task.Delay(10000);
                    break;
                case false:
                    executeStatus = true;
                    logger.LogWarning("This is startup work.");
                    await SendPingToAllAddresses(stoppingToken);
                    logger.LogWarning("Startup work has been ended.");
                    break;
                case true:
                    //Periodical work
                    await SendPingToAllAddresses(stoppingToken);
                    break;

            }
        }
        logger.LogInformation("DeviceStatusService is stopping.");
        await Task.CompletedTask;
    }
    private bool isBusy = false;
    private async Task SendPingToAllAddresses(CancellationToken stoppingToken)
    {
        //logger.LogInformation("BeginDoWorks");
        if (!isBusy)
        {
            isBusy = true;
            try
            {
                using (var db = new DB())
                {
                    var addresses = db.Devices.AsNoTracking().Where(x => x.IsActive).SelectMany(x => x.DeviceAddresses).Where(x => x.PeriodicPingCheck).ToList();
                    foreach (var addr in addresses)
                    {
                        try
                        {
                            logger.LogInformation("Ping to " + addr.Addr);
                            var ping = new Ping();
                            var reply = ping.Send(addr!.Addr ?? "");
                            logger.LogInformation("Ping to " + addr.Addr + " is " + reply.Status);
                            AddOrUpdateDeviceAddressStatus(addr.DeviceAddrId, reply.Status == IPStatus.Success);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error in SendPingToAllAddresses:" + ex.Message + Environment.NewLine + ex.StackTrace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SendPingToAllAddresses:" + ex.Message + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                try
                {
                    //Wait for 1 minute before next work
                    await Task.Delay(1000 * 60 * 1, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    logger.LogWarning("Bye bye error!");
                }
                isBusy = false;
            }
        }
        //logger.LogInformation("EndDoWorks");
    }
}
