using FBC.Devices.API.Data.Repositories;
using FBC.Devices.API.Models;
using FBC.Mediator;

namespace FBC.Devices.API.Features.AddrType;

public sealed class AddrTypeGetAll
{
    public record Query() : IRequest<List<Models.AddrType>>;

    internal sealed class Handler(AddrTypeRepository repo)
        : IRequestHandler<Query, List<Models.AddrType>>
    {
        public async Task<List<Models.AddrType>> Handle(Query request, CancellationToken token = default)
        {
            var result = await repo.GetListAsync(
                orderBy: q => q.OrderBy(x => x.Name),
                enableTracking: false,
                cancellationToken: token);
            return result.Items.ToList();
        }
    }
}
