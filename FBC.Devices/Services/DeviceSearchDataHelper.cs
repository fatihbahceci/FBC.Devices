using FBC.Devices.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FBC.Devices.Services;

internal record SearchCriteriaInfo(int Index, DeviceSearchDataTable Table, string FieldName);

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
    public static readonly List<(DeviceSearchDataTable Table, string FieldName)> SearchCriterias = new List<(DeviceSearchDataTable, string)>
    {
        (DeviceSearchDataTable.Device, nameof(Device.Name)),
        (DeviceSearchDataTable.Device, nameof(Device.Description)),
        (DeviceSearchDataTable.Device, nameof(Device.DeviceModel)),
        (DeviceSearchDataTable.Device, nameof(Device.SerialNumber)),
        (DeviceSearchDataTable.Device, nameof(Device.Location)),
        (DeviceSearchDataTable.Device, nameof(Device.Note)),
        (DeviceSearchDataTable.Device, nameof(Device.IsActive)),
        (DeviceSearchDataTable.DeviceType, nameof(DeviceType.Name)),
        (DeviceSearchDataTable.DeviceGroup, nameof(DeviceGroup.Name)),
        (DeviceSearchDataTable.DeviceAddress, nameof(DeviceAddr.Addr)),
        (DeviceSearchDataTable.DeviceAddress, nameof(DeviceAddr.Username)),
        //(DeviceSearchDataTable.DeviceAddress, nameof(DeviceAddr.Password)), // Exclude password from search
        (DeviceSearchDataTable.DeviceAddress, nameof(DeviceAddr.PeriodicPingCheck)),
        (DeviceSearchDataTable.DeviceAddressType, nameof(AddrType.Name)),
    };
    public static readonly List<SearchCriteriaInfo> SearchCriteriaKeys = SearchCriterias
        .Select((value, index) => new SearchCriteriaInfo(index + 1, value.Table, value.FieldName))
        .ToList();

    public static List<int> GetDeviceIds(DB db, List<SearchCriteriaInfo> fields, string filter)
    {
        var result = new List<int>();
        var tables = fields.Select(x => x.Table).Distinct().ToList();
        var fieldNames = fields.Select(x => x.FieldName).Distinct().ToList();
        var q = from x in db.DeviceSearchMetas
                where tables.Contains(x.FieldTable)
                && fieldNames.Contains(x.FieldName)
                && (string.IsNullOrEmpty(filter) || x.FieldValue.ToLower().Contains(filter.ToLower()))
                select x.DeviceId;
        return q.Distinct().ToList();

    }
    public static List<DeviceSearchData> GenerateDeviceSearchDataList(Device device)
    {
        List<DeviceSearchData> r = new List<DeviceSearchData>();
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.Name), device.Name));
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.Description), device.Description));
        if (device.DeviceGroup != null)
        {
            r.Add(new DeviceSearchData(device.PrimaryKeyId, device.DeviceGroup));
        }
        if (device.DeviceType != null)
        {
            r.Add(new DeviceSearchData(device.PrimaryKeyId, device.DeviceType));
        }
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.DeviceModel), device.DeviceModel));
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.SerialNumber), device.SerialNumber));
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.Location), device.Location));
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.Note), device.Note));
        r.Add(new DeviceSearchData(device.PrimaryKeyId, nameof(device.IsActive), device.IsActive));
        if (device.DeviceAddresses?.Any() == true)
        {
            foreach (var addr in device.DeviceAddresses)
            {
                if (addr.AddrType != null)
                {
                    r.Add(new DeviceSearchData(device.PrimaryKeyId, addr.PrimaryKeyId, addr.AddrType));
                }
                r.Add(new DeviceSearchData(DeviceSearchDataTable.DeviceAddress, device.PrimaryKeyId, nameof(addr.Addr), addr.Addr)
                {
                    DeviceAddrId = addr.PrimaryKeyId
                });
                r.Add(new DeviceSearchData(DeviceSearchDataTable.DeviceAddress, device.PrimaryKeyId, nameof(addr.Username), addr.Username)
                {
                    DeviceAddrId = addr.PrimaryKeyId
                });
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
        var device = await GetDeviceWithFullData(db, devicePk, stoppingToken);
        if (device == null)
        {
            logger.LogWarning($"Device with ID {devicePk} not found, skipping.");
            return;
        }
        var list = GenerateDeviceSearchDataList(device);
        await SyncSearchDataFor(db, device.DeviceId, list, logger, stoppingToken);
    }
}
