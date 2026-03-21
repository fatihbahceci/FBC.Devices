using FBC.DBRepository;

namespace FBC.Devices.API.Models;

public class DeviceType : Entity<int, DeviceType>
{
    public string Name { get; set; } = "New Type";
    public string? Description { get; set; }
}
