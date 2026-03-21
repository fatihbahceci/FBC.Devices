using FBC.DBRepository;
using FBC.Devices.API.Models;

namespace FBC.Devices.API.Data.Repositories;

public class AddrTypeRepository : EFRepositoryBase<AddrType, int, AppDbContext>
{
    public AddrTypeRepository(AppDbContext context) : base(context) { }
}
