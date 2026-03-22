using FBC.Devices.API.Data;
using FBC.Devices.API.Data.Repositories;
using FBC.Devices.API.Services;
using FBC.Mediator;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Features.Search;

public sealed class SearchDevices
{
    public record Query(string? Filter, int[]? Fields, int Skip = 0, int Take = 10) : IRequest<SearchResult>;

    public record SearchResult(int TotalCount, List<Models.Device> Items);

    internal sealed class Handler(AppDbContext db, DeviceRepository deviceRepo)
        : IRequestHandler<Query, SearchResult>
    {
        public async Task<SearchResult> Handle(Query request, CancellationToken token = default)
        {
            var fields = DeviceSearchDataHelper.SearchCriteriaKeys;
            if (request.Fields?.Any() == true)
                fields = fields.Where(f => request.Fields.Contains(f.Index)).ToList();

            var deviceIds = DeviceSearchDataHelper.GetDeviceIds(db, fields, request.Filter ?? "");

            // Use GetByIdsAsync for fetching devices with includes
            var allDevices = await deviceRepo.GetByIdsAsync(
                deviceIds,
                include: q => q
                    .Include(d => d.DeviceType)
                    .Include(d => d.DeviceGroup)
                    .Include(d => d.DeviceAddresses).ThenInclude(a => a.AddrType!),
                enableTracking: false,
                cancellationToken: token);

            var totalCount = allDevices.Count;
            var items = allDevices
                .OrderBy(d => d.Name)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToList();

            return new SearchResult(totalCount, items);
        }
    }
}
