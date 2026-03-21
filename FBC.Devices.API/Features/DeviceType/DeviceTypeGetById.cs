using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceType;

public sealed class DeviceTypeGetById
{
    public record Query(int Id) : IRequest<Models.DeviceType?>;

    internal sealed class Handler(DeviceTypeRepository repo)
        : IRequestHandler<Query, Models.DeviceType?>
    {
        public async Task<Models.DeviceType?> Handle(Query request, CancellationToken token = default)
            => await repo.GetByIdAsync(request.Id, enableTracking: false, cancellationToken: token);
    }
}
