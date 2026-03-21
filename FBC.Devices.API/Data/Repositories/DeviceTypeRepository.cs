using FBC.DBRepository;
using FBC.Devices.API.Models;

namespace FBC.Devices.API.Data.Repositories;

public class DeviceTypeRepository : EFRepositoryBase<DeviceType, int, AppDbContext>
{
    public DeviceTypeRepository(AppDbContext context) : base(context) { }
}
