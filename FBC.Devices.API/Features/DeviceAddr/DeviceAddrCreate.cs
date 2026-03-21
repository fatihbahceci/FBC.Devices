using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceAddr;

public sealed class DeviceAddrCreate
{
    public record Command(int DeviceId, int AddrTypeId, string? Addr, string? Username, string? Password, bool PeriodicPingCheck) : IRequest<int>;

    internal sealed class Handler(DeviceAddrRepository repo)
        : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken token = default)
        {
            var entity = new Models.DeviceAddr
            {
                DeviceId = request.DeviceId,
                AddrTypeId = request.AddrTypeId,
                Addr = request.Addr,
                Username = request.Username,
                Password = request.Password,
                PeriodicPingCheck = request.PeriodicPingCheck
            };
            await repo.ApplyOperation(EntityOperation.Create, entity, alsoValidate: true);
            return entity.Id;
        }
    }
}
