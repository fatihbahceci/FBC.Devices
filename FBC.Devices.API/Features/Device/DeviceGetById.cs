using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Features.Device;

public sealed class DeviceGetById
{
    public record Query(int Id) : IRequest<Models.Device?>;

    internal sealed class Handler(DeviceRepository repo)
        : IRequestHandler<Query, Models.Device?>
    {
        public async Task<Models.Device?> Handle(Query request, CancellationToken token = default)
        {
            return await repo.GetAsync(
                predicate: d => d.Id == request.Id,
                include: q => q
                    .Include(d => d.DeviceType)
                    .Include(d => d.DeviceGroup)
                    .Include(d => d.DeviceAddresses).ThenInclude(a => a.AddrType!),
                enableTracking: false,
                cancellationToken: token);
        }
    }
}
