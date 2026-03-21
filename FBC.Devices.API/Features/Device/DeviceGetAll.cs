using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Features.Device;

public sealed class DeviceGetAll
{
    public record Query(int PageNumber = 0, int ItemsPerPage = 25) : IRequest<PaginateResponseModel<Models.Device>>;

    internal sealed class Handler(DeviceRepository repo)
        : IRequestHandler<Query, PaginateResponseModel<Models.Device>>
    {
        public async Task<PaginateResponseModel<Models.Device>> Handle(Query request, CancellationToken token = default)
        {
            return await repo.GetListAsync(
                orderBy: q => q.OrderBy(x => x.Name),
                include: q => q
                    .Include(d => d.DeviceType)
                    .Include(d => d.DeviceGroup)
                    .Include(d => d.DeviceAddresses),
                pageNumber: request.PageNumber,
                itemsPerPage: request.ItemsPerPage,
                enableTracking: false,
                cancellationToken: token);
        }
    }
}
