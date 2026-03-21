using System.ComponentModel.DataAnnotations.Schema;
using FBC.DBRepository;

namespace FBC.Devices.API.Models;

public class DeviceAddr : Entity<int, DeviceAddr>
{
    [ForeignKey(nameof(Device))]
    public int DeviceId { get; set; }

    [ForeignKey(nameof(AddrType))]
    public int AddrTypeId { get; set; }
    public AddrType? AddrType { get; set; }

    public string? Addr { get; set; } = "http://";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool PeriodicPingCheck { get; set; }

    internal void AdjustData()
    {
        if (AddrTypeId == 0)
        {
            AddrType = null;
        }
    }
}
