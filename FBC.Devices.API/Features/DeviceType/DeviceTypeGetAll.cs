using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceType;

public sealed class DeviceTypeGetAll
{
    public record Query() : IRequest<List<Models.DeviceType>>;

    internal sealed class Handler(DeviceTypeRepository repo)
        : IRequestHandler<Query, List<Models.DeviceType>>
    {
        public async Task<List<Models.DeviceType>> Handle(Query request, CancellationToken token = default)
        {
            var result = await repo.GetListAsync(
                orderBy: q => q.OrderBy(x => x.Name),
                enableTracking: false,
                cancellationToken: token);
            return result.Items.ToList();
        }
    }
}
