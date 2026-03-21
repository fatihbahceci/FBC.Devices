using FBC.Devices.API.Data;
using FBC.Devices.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Services;

public record SearchCriteriaInfo(int Index, DeviceSearchDataTable Table, string FieldName);

public static class DeviceSearchDataHelper
{
    public static readonly List<(DeviceSearchDataTable Table, string FieldName)> SearchCriterias = new()
    {
        (DeviceSearchDataTable.Device, nameof(Device.Name)),
        (DeviceSearchDataTable.Device, nameof(Device.Description)),
        (DeviceSearchDataTable.Device, nameof(Device.DeviceModel)),
        (DeviceSearchDataTable.Device, nameof(Device.SerialNumber)),
        (DeviceSearchDataTable.Device, nameof(Device.Location)),
        (DeviceSearchDataTable.Device, nameof(Device.Note)),
        (DeviceSearchDataTable.Device, nameof(Device.IsActive)),
        (DeviceSearchDataTable.DeviceType, nameof(Models.DeviceType.Name)),
        (DeviceSearchDataTable.DeviceGroup, nameof(Models.DeviceGroup.Name)),
        (DeviceSearchDataTable.DeviceAddress, nameof(DeviceAddr.Addr)),
        (DeviceSearchDataTable.DeviceAddress, nameof(DeviceAddr.Username)),
        (DeviceSearchDataTable.DeviceAddress, nameof(DeviceAddr.PeriodicPingCheck)),
        (DeviceSearchDataTable.DeviceAddressType, nameof(Models.AddrType.Name)),
    };

    public static readonly List<SearchCriteriaInfo> SearchCriteriaKeys = SearchCriterias
        .Select((value, index) => new SearchCriteriaInfo(index + 1, value.Table, value.FieldName))
        .ToList();

    public static async Task<Device?> GetDeviceWithFullData(AppDbContext db, int deviceId, CancellationToken ct)
    {
        return await db.Devices.AsNoTracking()
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceGroup)
            .Include(d => d.DeviceAddresses).ThenInclude(da => da.AddrType!)
            .FirstOrDefaultAsync(d => d.Id == deviceId, ct);
    }

    public static List<int> GetDeviceIds(AppDbContext db, List<SearchCriteriaInfo> fields, string filter)
    {
        var tables = fields.Select(x => x.Table).Distinct().ToList();
        var fieldNames = fields.Select(x => x.FieldName).Distinct().ToList();
        return (from x in db.DeviceSearchMetas
                where tables.Contains(x.FieldTable)
                    && fieldNames.Contains(x.FieldName)
                    && (string.IsNullOrEmpty(filter) || x.FieldValue.ToLower().Contains(filter.ToLower()))
                select x.DeviceId).Distinct().ToList();
    }

    public static List<DeviceSearchData> GenerateDeviceSearchDataList(Device device)
    {
        var r = new List<DeviceSearchData>
        {
            new(device.Id, nameof(device.Name), device.Name),
            new(device.Id, nameof(device.Description), device.Description)
        };

        if (device.DeviceGroup != null)
            r.Add(new DeviceSearchData(device.Id, device.DeviceGroup));
        if (device.DeviceType != null)
            r.Add(new DeviceSearchData(device.Id, device.DeviceType));

        r.Add(new DeviceSearchData(device.Id, nameof(device.DeviceModel), device.DeviceModel));
        r.Add(new DeviceSearchData(device.Id, nameof(device.SerialNumber), device.SerialNumber));
        r.Add(new DeviceSearchData(device.Id, nameof(device.Location), device.Location));
        r.Add(new DeviceSearchData(device.Id, nameof(device.Note), device.Note));
        r.Add(new DeviceSearchData(device.Id, nameof(device.IsActive), device.IsActive));

        if (device.DeviceAddresses?.Any() == true)
        {
            foreach (var addr in device.DeviceAddresses)
            {
                if (addr.AddrType != null)
                    r.Add(new DeviceSearchData(device.Id, addr.Id, addr.AddrType));

                r.Add(new DeviceSearchData(DeviceSearchDataTable.DeviceAddress, device.Id, nameof(addr.Addr), addr.Addr) { DeviceAddrId = addr.Id });
                r.Add(new DeviceSearchData(DeviceSearchDataTable.DeviceAddress, device.Id, nameof(addr.Username), addr.Username) { DeviceAddrId = addr.Id });
                r.Add(new DeviceSearchData(DeviceSearchDataTable.DeviceAddress, device.Id, nameof(addr.PeriodicPingCheck), addr.PeriodicPingCheck) { DeviceAddrId = addr.Id });
            }
        }
        return r;
    }

    public static async Task SyncDeviceSearchData(AppDbContext db, int devicePk, ILogger logger, CancellationToken ct)
    {
        var device = await GetDeviceWithFullData(db, devicePk, ct);
        if (device == null)
        {
            logger.LogWarning("Device with ID {DeviceId} not found, skipping.", devicePk);
            return;
        }
        var generated = GenerateDeviceSearchDataList(device);
        await SyncSearchDataFor(db, device.Id, generated, logger, ct);
    }

    private static async Task SyncSearchDataFor(AppDbContext db, int deviceId, List<DeviceSearchData> generated, ILogger logger, CancellationToken ct)
    {
        var existing = await db.DeviceSearchMetas.Where(d => d.DeviceId == deviceId).ToListAsync(ct);
        var comparer = new DeviceSearchDataKeyComparer();

        var willDelete = existing.Except(generated, comparer).ToList();
        var willInsert = generated.Except(existing, comparer).ToList();
        var willUpdate = existing
            .Join(generated, e => e, g => g, (e, g) => new { existing = e, generated = g }, comparer)
            .Where(x => x.existing.FieldValue != x.generated.FieldValue)
            .ToList();

        if (willDelete.Any())
        {
            db.DeviceSearchMetas.RemoveRange(willDelete);
            logger.LogInformation("Device ID {DeviceId}: Deleting {Count} search data entries.", deviceId, willDelete.Count);
        }
        if (willInsert.Any())
        {
            await db.DeviceSearchMetas.AddRangeAsync(willInsert, ct);
            logger.LogInformation("Device ID {DeviceId}: Inserting {Count} search data entries.", deviceId, willInsert.Count);
        }
        if (willUpdate.Any())
        {
            logger.LogInformation("Device ID {DeviceId}: Updating {Count} search data entries.", deviceId, willUpdate.Count);
            foreach (var item in willUpdate)
            {
                item.existing.FieldValue = item.generated.FieldValue;
                db.Entry(item.existing).State = EntityState.Modified;
            }
        }
        if (willDelete.Any() || willInsert.Any() || willUpdate.Any())
        {
            logger.LogInformation("Device ID {DeviceId}: Saving changes to database.", deviceId);
            await db.SaveChangesAsync(ct);
        }
    }
}
