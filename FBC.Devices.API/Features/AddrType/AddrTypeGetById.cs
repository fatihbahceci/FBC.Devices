using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.AddrType;

public sealed class AddrTypeGetById
{
    public record Query(int Id) : IRequest<Models.AddrType?>;

    internal sealed class Handler(AddrTypeRepository repo)
        : IRequestHandler<Query, Models.AddrType?>
    {
        public async Task<Models.AddrType?> Handle(Query request, CancellationToken token = default)
            => await repo.GetByIdAsync(request.Id, enableTracking: false, cancellationToken: token);
    }
}
