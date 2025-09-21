using FBC.Devices.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FBC.Devices.Services;

internal static class DeviceSearchDataHelper
{
    public static async Task<Device?> GetDeviceWithFullData(DB db, int deviceId, CancellationToken ct)
    {
        return await db.Devices.AsNoTracking()
            /*
             * nameof(DeviceType),
             * nameof(DeviceGroup),
             * nameof(Device.DeviceAddresses),
             * nameof(Device.DeviceAddresses) + "."+ nameof(AddrType)
             */
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceGroup)
            .Include(d => d.DeviceAddresses).ThenInclude(da => da.AddrType)
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId, ct);
    }
    public static List<DeviceSearchData> GenerateDeviceSearchDataList(Device device)
    {
        List<DeviceSearchData> r = new List<DeviceSearchData>();
        //[Key]
        //public int DeviceId { get; set; }
        //public int PrimaryKeyId => DeviceId;
        //public string Name { get; set; }
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.Name), device.Name));
        //public string? Description { get; set; }
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.Description), device.Description));
        //[ForeignKey(nameof(DeviceGroup))]
        //public int? DeviceGroupId { get; set; }
        //public DeviceGroup? DeviceGroup { get; set; }
        if (device.DeviceGroup != null)
        {
            r.Add(new DeviceSearchData(device.PrimaryKeyId, device.DeviceGroup));
        }
        //[ForeignKey(nameof(DeviceType))]
        //public int? DeviceTypeId { get; set; }
        //public DeviceType? DeviceType { get; set; }
        if (device.DeviceType != null)
        {
            r.Add(new DeviceSearchData(device.PrimaryKeyId, device.DeviceType));
        }
        //public string? DeviceModel { get; set; }
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.DeviceModel), device.DeviceModel));
        //public string? SerialNumber { get; set; }
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.SerialNumber), device.SerialNumber));
        //public string? Location { get; set; }
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.Location), device.Location));
        //public string? Note { get; set; }
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.Note), device.Note));
        //public bool IsActive { get; set; }
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.IsActive), device.IsActive));
        //public virtual List<DeviceAddr> DeviceAddresses
        if (device.DeviceAddresses?.Any() == true)
        {
            foreach (var addr in device.DeviceAddresses)
            {
                // [Key]
                // public int DeviceAddrId { get; set; }
                // public int PrimaryKeyId => DeviceAddrId;
                // [ForeignKey(nameof(Device))]
                // public int DeviceId { get; set; }
                // //public Device? Device { get; set; }
                // [ForeignKey(nameof(AddrType))]
                // public int AddrTypeId { get; set; }
                // public AddrType? AddrType { get; set; }
                if (addr.AddrType != null)
                {
                    r.Add(new DeviceSearchData(device.PrimaryKeyId, addr.PrimaryKeyId, addr.AddrType));
                }
                // public string? Addr { get; set; }
                r.Add(new DeviceSearchData(DeviceSearchDataTable.DeviceAddress, device.PrimaryKeyId, nameof(addr.Addr), addr.Addr)
                {
                    DeviceAddrId = addr.PrimaryKeyId
                });
                // public string? Username { get; set; }
                r.Add(new DeviceSearchData(DeviceSearchDataTable.DeviceAddress, device.PrimaryKeyId, nameof(addr.Username), addr.Username)
                {
                    DeviceAddrId = addr.PrimaryKeyId
                });
                // public string? Password { get; set; }
                //r.Add(new DeviceSearchData(DeviceSearchDataTable.DeviceAddress, device.PrimaryKeyId, nameof(addr.Password), addr.Password)
                //{
                //    DeviceAddrId = addr.PrimaryKeyId
                //});
                // public bool PeriodicPingCheck { get; set; }
                r.Add(new DeviceSearchData(DeviceSearchDataTable.DeviceAddress, device.PrimaryKeyId, nameof(addr.PeriodicPingCheck), addr.PeriodicPingCheck)
                {
                    DeviceAddrId = addr.PrimaryKeyId
                });
            }
        }
        return r;
    }
    private static async Task SyncSearchDataFor(DB db, int deviceId, List<DeviceSearchData> generated, ILogger logger, CancellationToken ct)
    {
        var existing = await db.DeviceSearchMetas.Where(d => d.DeviceId == deviceId).ToListAsync(ct);
        var comparer = new DeviceSearchDataKeyComparer();

        var willDelete = existing.Except(generated, comparer).ToList();
        var willInsert = generated.Except(existing, comparer).ToList();
        // existing.IsKeysEqual(generated) and existing.FieldValue != generated.FieldValue
        var willUpdate = existing
            .Join(generated, e => e, g => g, (e, g) => new { existing = e, generated = g }, comparer)
            .Where(x => x.existing.FieldValue != x.generated.FieldValue)
            .ToList();
        if (willDelete.Any())
        {
            db.DeviceSearchMetas.RemoveRange(willDelete);
            logger.LogInformation($"Device ID {deviceId}: Deleting {willDelete.Count} search data entries.");
        }
        if (willInsert.Any())
        {
            await db.DeviceSearchMetas.AddRangeAsync(willInsert, ct);
            logger.LogInformation($"Device ID {deviceId}: Inserting {willInsert.Count} search data entries.");
        }
        if (willUpdate.Any())
        {
            logger.LogInformation($"Device ID {deviceId}: Updating {willUpdate.Count} search data entries.");
            foreach (var item in willUpdate)
            {
                item.existing.FieldValue = item.generated.FieldValue;
                db.Entry(item.existing).State = EntityState.Modified; //Explicitly mark as modified. Not required, but just to be sure.
            }
        }
        if (willDelete.Any() || willInsert.Any() || willUpdate.Any())
        {
            logger.LogInformation($"Device ID {deviceId}: Saving changes to database.");
            await db.SaveChangesAsync(ct);
        }
    }
    public static async Task SyncDeviceSearchData(DB db, int devicePk, ILogger logger, CancellationToken stoppingToken)
    {
        var device = await DeviceSearchDataHelper.GetDeviceWithFullData(db, devicePk, stoppingToken);
        if (device == null)
        {
            logger.LogWarning($"Device with ID {devicePk} not found, skipping.");
            return;
        }
        var list = DeviceSearchDataHelper.GenerateDeviceSearchDataList(device);
        await SyncSearchDataFor(db, device.DeviceId, list, logger, stoppingToken);
    }
}
