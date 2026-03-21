using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Models;

public enum DeviceSearchDataTable
{
    Device,
    DeviceType,
    DeviceGroup,
    DeviceAddress,
    DeviceAddressType
}

[Index(nameof(DeviceId), Name = "IX_DeviceSearchData_DeviceId")]
[Index(nameof(FieldTable), nameof(DeviceId), nameof(FieldName), IsUnique = false, Name = "IDX_DeviceSearchData_Freq_Fields")]
[Index(nameof(FieldTable), nameof(DeviceId), nameof(DeviceTypeId), nameof(DeviceGroupId),
       nameof(DeviceAddrId), nameof(DeviceAddrTypeId), nameof(FieldName),
       IsUnique = true, Name = "UX_DeviceSearchData_Key")]
public class DeviceSearchData
{
    [Key]
    public int DeviceSearchDataId { get; set; }
    public DeviceSearchDataTable FieldTable { get; set; }
    public int DeviceId { get; set; }
    public int? DeviceTypeId { get; set; }
    public int? DeviceGroupId { get; set; }
    public int? DeviceAddrId { get; set; }
    public int? DeviceAddrTypeId { get; set; }

    [MaxLength(255)]
    public string FieldName { get; set; } = string.Empty;
    public string FieldValue { get; set; } = string.Empty;

    public DeviceSearchData() { }

    public DeviceSearchData(DeviceSearchDataTable table, int deviceId, string fieldName, object? fieldValue) : this()
    {
        FieldTable = table;
        DeviceId = deviceId;
        FieldName = fieldName;
        FieldValue = fieldValue switch
        {
            null => "",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => fieldValue.ToString() ?? ""
        };
    }

    public DeviceSearchData(int deviceId, string fieldName, object? fieldValue)
        : this(DeviceSearchDataTable.Device, deviceId, fieldName, fieldValue) { }

    public DeviceSearchData(int deviceId, DeviceGroup deviceGroup)
        : this(DeviceSearchDataTable.DeviceGroup, deviceId, nameof(DeviceGroup.Name), deviceGroup.Name)
    {
        DeviceGroupId = deviceGroup.Id;
    }

    public DeviceSearchData(int deviceId, DeviceType deviceType)
        : this(DeviceSearchDataTable.DeviceType, deviceId, nameof(DeviceType.Name), deviceType.Name)
    {
        DeviceTypeId = deviceType.Id;
    }

    public DeviceSearchData(int deviceId, int addrId, AddrType addrType)
        : this(DeviceSearchDataTable.DeviceAddressType, deviceId, nameof(AddrType.Name), addrType.Name)
    {
        DeviceAddrId = addrId;
        DeviceAddrTypeId = addrType.Id;
    }

    public bool IsKeysEqual(DeviceSearchData other)
    {
        return FieldTable == other.FieldTable
            && DeviceId == other.DeviceId
            && DeviceTypeId == other.DeviceTypeId
            && DeviceGroupId == other.DeviceGroupId
            && DeviceAddrId == other.DeviceAddrId
            && DeviceAddrTypeId == other.DeviceAddrTypeId
            && FieldName == other.FieldName;
    }
}

public class DeviceSearchDataKeyComparer : IEqualityComparer<DeviceSearchData>
{
    public bool Equals(DeviceSearchData? x, DeviceSearchData? y)
    {
        if (x == null || y == null) return false;
        return x.IsKeysEqual(y);
    }

    public int GetHashCode(DeviceSearchData obj)
    {
        return HashCode.Combine(obj.FieldTable, obj.DeviceId, obj.DeviceTypeId,
            obj.DeviceGroupId, obj.DeviceAddrId, obj.DeviceAddrTypeId, obj.FieldName);
    }
}
