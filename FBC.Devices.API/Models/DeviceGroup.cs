using FBC.DBRepository;

namespace FBC.Devices.API.Models;

public class DeviceGroup : Entity<int, DeviceGroup>
{
    public string Name { get; set; } = "New Group";
    public string? Description { get; set; }
}
