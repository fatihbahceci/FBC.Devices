using FBC.DBRepository;
using FBC.Devices.API.Models;

namespace FBC.Devices.API.Data.Repositories;

public class DeviceGroupRepository : EFRepositoryBase<DeviceGroup, int, AppDbContext>
{
    public DeviceGroupRepository(AppDbContext context, ICurrentUserProvider? currentUserProvider = null)
        : base(context, currentUserProvider) { }
}
