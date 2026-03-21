using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceGroup;

public sealed class DeviceGroupGetById
{
    public record Query(int Id) : IRequest<Models.DeviceGroup?>;

    internal sealed class Handler(DeviceGroupRepository repo)
        : IRequestHandler<Query, Models.DeviceGroup?>
    {
        public async Task<Models.DeviceGroup?> Handle(Query request, CancellationToken token = default)
            => await repo.GetByIdAsync(request.Id, enableTracking: false, cancellationToken: token);
    }
}
