using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceAddr;

public sealed class DeviceAddrUpdate
{
    public record Command(int Id, int AddrTypeId, string? Addr, string? Username, string? Password, bool PeriodicPingCheck) : IRequest;

    internal sealed class Handler(DeviceAddrRepository repo)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken: token)
                ?? throw new KeyNotFoundException($"DeviceAddr {request.Id} not found");

            entity.AddrTypeId = request.AddrTypeId;
            entity.Addr = request.Addr;
            entity.Username = request.Username;
            entity.Password = request.Password;
            entity.PeriodicPingCheck = request.PeriodicPingCheck;
            await repo.ApplyOperation(EntityOperation.Update, entity, alsoValidate: true);
        }
    }
}
