using FBC.DBRepository;

namespace FBC.Devices.API.Models;

public class AddrType : Entity<int, AddrType>
{
    public string Name { get; set; } = "New Address Type";
}
