using FBC.DBRepository;
using FBC.Devices.API.Models;

namespace FBC.Devices.API.Data.Repositories;

public class DeviceRepository : EFRepositoryBase<Device, int, AppDbContext>
{
    public DeviceRepository(AppDbContext context) : base(context) { }
}
