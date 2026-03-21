using FBC.DBRepository;
using FBC.Devices.API.Data;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Features.Device;

public sealed class DeviceDelete
{
    public record Command(int Id) : IRequest;

    internal sealed class Handler(DeviceRepository deviceRepo, AppDbContext db)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var device = await deviceRepo.GetByIdAsync(request.Id, cancellationToken: token);
            if (device is null) return;

            // Delete child addresses first
            var addresses = await db.DeviceAddresses
                .Where(a => a.DeviceId == request.Id)
                .ToListAsync(token);
            if (addresses.Any())
                db.DeviceAddresses.RemoveRange(addresses);

            await deviceRepo.ApplyOperation(EntityOperation.Delete, device, alsoValidate: false, deletePermanent: true);
        }
    }
}
