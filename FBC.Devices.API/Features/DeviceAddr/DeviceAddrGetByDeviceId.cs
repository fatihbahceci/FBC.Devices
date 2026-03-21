using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Features.DeviceAddr;

public sealed class DeviceAddrGetByDeviceId
{
    public record Query(int DeviceId) : IRequest<List<Models.DeviceAddr>>;

    internal sealed class Handler(DeviceAddrRepository repo)
        : IRequestHandler<Query, List<Models.DeviceAddr>>
    {
        public async Task<List<Models.DeviceAddr>> Handle(Query request, CancellationToken token = default)
        {
            var result = await repo.GetListAsync(
                predicate: a => a.DeviceId == request.DeviceId,
                include: q => q.Include(a => a.AddrType!),
                enableTracking: false,
                cancellationToken: token);
            return result.Items.ToList();
        }
    }
}
