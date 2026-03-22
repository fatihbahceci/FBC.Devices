using FBC.DBRepository;
using FBC.Devices.API.Models;

namespace FBC.Devices.API.Data.Repositories;

public class UserRepository : EFRepositoryBase<AppUser, int, AppDbContext>
{
    public UserRepository(AppDbContext context, ICurrentUserProvider? currentUserProvider = null)
        : base(context, currentUserProvider) { }
}
