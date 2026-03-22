using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.Device;

public sealed class DeviceDelete
{
    public record Command(int Id) : IRequest;

    internal sealed class Handler(DeviceRepository deviceRepo, DeviceAddrRepository deviceAddrRepo)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var device = await deviceRepo.GetByIdAsync(request.Id, cancellationToken: token);
            if (device is null) return;

            // Delete child addresses first via repository
            var addresses = await deviceAddrRepo.GetListAsync(a => a.DeviceId == request.Id, cancellationToken: token);
            if (addresses.Items.Any())
                await deviceAddrRepo.ApplyOperationRange(EntityOperation.Delete, addresses.Items, false, true);

            await deviceRepo.ApplyOperation(EntityOperation.Delete, device, alsoValidate: false, deletePermanent: true);
        }
    }
}
