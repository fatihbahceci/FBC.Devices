using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FBC.DBRepository;

namespace FBC.Devices.API.Models;

public class Device : Entity<int, Device>
{
    public string Name { get; set; } = "New Device";
    public string? Description { get; set; }

    [ForeignKey(nameof(DeviceGroup))]
    public int? DeviceGroupId { get; set; }
    public DeviceGroup? DeviceGroup { get; set; }

    [ForeignKey(nameof(DeviceType))]
    public int? DeviceTypeId { get; set; }
    public DeviceType? DeviceType { get; set; }

    public string? DeviceModel { get; set; }
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }

    private List<DeviceAddr>? _deviceAddresses;
    public virtual List<DeviceAddr> DeviceAddresses
    {
        get => _deviceAddresses ??= new List<DeviceAddr>();
        set => _deviceAddresses = value ?? new List<DeviceAddr>();
    }

    public void AdjustData(bool validate)
    {
        if (DeviceTypeId == 0)
        {
            DeviceTypeId = null;
            DeviceType = null;
        }
        if (DeviceGroupId == 0)
        {
            DeviceGroupId = null;
            DeviceGroup = null;
        }
        if (DeviceAddresses?.Any() == true)
        {
            foreach (var addr in DeviceAddresses)
            {
                addr.DeviceId = Id;
                addr.AdjustData();
            }
        }
        if (validate)
        {
            if (DeviceAddresses?.Any(x => string.IsNullOrEmpty(x.Addr)) == true)
                throw new ValidationException("Device Address is not valid!");
            if (DeviceAddresses?.Any(x => x.AddrTypeId <= 0) == true)
                throw new ValidationException("Device Address Type is not valid!");
        }
    }
}
