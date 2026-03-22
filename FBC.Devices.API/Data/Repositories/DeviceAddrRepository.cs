using FBC.DBRepository;
using FBC.Devices.API.Models;

namespace FBC.Devices.API.Data.Repositories;

public class DeviceAddrRepository : EFRepositoryBase<DeviceAddr, int, AppDbContext>
{
    public DeviceAddrRepository(AppDbContext context, ICurrentUserProvider? currentUserProvider = null)
        : base(context, currentUserProvider) { }
}
