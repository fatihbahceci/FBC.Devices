using FBC.DBRepository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FBC.Devices.API.Models;

public class DeviceAddr : APIBaseEntity<DeviceAddr>
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

    public override async Task CheckDataForAsync(EntityOperation operation, bool alsoValidate, IQueryable<DeviceAddr> query)
    {
        if (AddrTypeId == 0)
        {
            AddrType = null;
        }
        if (alsoValidate)
        {
            if (string.IsNullOrEmpty(Addr))
                throw new ValidationException("Device Address is not valid!");
            if (AddrTypeId <= 0)
                throw new ValidationException("Device Address Type is not valid!");
            //if (query != null)
            //{
            //    switch (operation)
            //    {
            //        case EntityOperation.Create:
            //        case EntityOperation.Update:
            //            var exists = await query.AnyAsync(x => x.Id != Id && x.DeviceId == DeviceId && x.AddrTypeId == AddrTypeId);
            //            if (exists)
            //                throw new ValidationException("Device Address Type already exists for this device!");
            //            break;
            //    }
            //}
        }
    }
}
