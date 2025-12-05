using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FBC.Devices.DBModels;

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
    /// <summary>
    /// Null: No binding
    /// </summary>
    public int? DeviceTypeId { get; set; }
    /// <summary>
    /// Null: No binding
    /// </summary>
    public int? DeviceGroupId { get; set; }
    /// <summary>
    /// Null: No binding
    /// </summary>
    public int? DeviceAddrId { get; set; }
    /// <summary>
    /// Null: No binding
    /// </summary>
    public int? DeviceAddrTypeId { get; set; }

    [MaxLength(255)]
    public string FieldName { get; set; } = string.Empty;
    /// <summary>
    /// Stores the value of the Field segment. Unlimited length for search purposes. (Sqlite)
    /// </summary>
    public string FieldValue { get; set; } = string.Empty;
    public DeviceSearchData()
    {
    }
    public DeviceSearchData(DeviceSearchDataTable table, int deviceId, string fieldName, object? fieldValue) : this()
    {
        this.FieldTable = table;
        this.DeviceId = deviceId;
        this.FieldName = fieldName;
        //Is this conversion correct for searching?
        this.FieldValue = fieldValue switch
        {
            null => "",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => fieldValue.ToString() ?? ""
        };
    }
    public DeviceSearchData(int deviceId, string fieldName, object? fieldValue) : this(DeviceSearchDataTable.Device, deviceId, fieldName, fieldValue)
    {
    }

    public DeviceSearchData(int deviceId, DeviceGroup deviceGroup) : this(DeviceSearchDataTable.DeviceGroup, deviceId, nameof(deviceGroup.Name), deviceGroup.Name)
    {
        this.DeviceGroupId = deviceGroup.DeviceGroupId;
    }

    public DeviceSearchData(int deviceId, DeviceType deviceType) : this(DeviceSearchDataTable.DeviceType, deviceId, nameof(deviceType.Name), deviceType.Name)
    {
        this.DeviceTypeId = deviceType.DeviceTypeId;
    }

    public DeviceSearchData(int deviceId, int addrId, AddrType addrType) : this(DeviceSearchDataTable.DeviceAddressType, deviceId, nameof(addrType.Name), addrType.Name)
    {
        this.DeviceAddrId = addrId;
        this.DeviceAddrTypeId = addrType.AddrTypeId;
    }
    public string GetKeysStr()
    {
        return $"T {FieldTable} | D {DeviceId} | DT {DeviceTypeId} | DG {DeviceGroupId} | DA {DeviceAddrId} | DAT {DeviceAddrTypeId} | F {FieldName}";
    }
    public bool IsKeysEqual(DeviceSearchData other)
    {
        return this.FieldTable == other.FieldTable
            && this.DeviceId == other.DeviceId
            && this.DeviceTypeId == other.DeviceTypeId
            && this.DeviceGroupId == other.DeviceGroupId
            && this.DeviceAddrId == other.DeviceAddrId
            && this.DeviceAddrTypeId == other.DeviceAddrTypeId
            && this.FieldName == other.FieldName;
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
        // Key alanlarının hepsini hash’e dahil et
        return HashCode.Combine(obj.FieldTable,
                                obj.DeviceId,
                                obj.DeviceTypeId,
                                obj.DeviceGroupId,
                                obj.DeviceAddrId,
                                obj.DeviceAddrTypeId,
                                obj.FieldName);
    }
}