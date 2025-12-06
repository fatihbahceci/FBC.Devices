namespace FBC.Devices.API.Features.Devices.Models;

public class DeviceRequestModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DeviceGroupId { get; set; }
    public int? DeviceTypeId { get; set; }
    public string? DeviceModel { get; set; }
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }
}
