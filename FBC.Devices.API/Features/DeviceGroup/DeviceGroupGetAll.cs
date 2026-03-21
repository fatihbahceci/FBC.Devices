using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceGroup;

public sealed class DeviceGroupGetAll
{
    public record Query() : IRequest<List<Models.DeviceGroup>>;

    internal sealed class Handler(DeviceGroupRepository repo)
        : IRequestHandler<Query, List<Models.DeviceGroup>>
    {
        public async Task<List<Models.DeviceGroup>> Handle(Query request, CancellationToken token = default)
        {
            var result = await repo.GetListAsync(
                orderBy: q => q.OrderBy(x => x.Name),
                enableTracking: false,
                cancellationToken: token);
            return result.Items.ToList();
        }
    }
}
