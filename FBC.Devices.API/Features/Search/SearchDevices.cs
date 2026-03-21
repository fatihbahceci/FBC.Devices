using FBC.Devices.API.Data;
using FBC.Devices.API.Services;
using FBC.Mediator;
using Microsoft.EntityFrameworkCore;

namespace FBC.Devices.API.Features.Search;

public sealed class SearchDevices
{
    public record Query(string? Filter, int[]? Fields, int Skip = 0, int Take = 10) : IRequest<SearchResult>;

    public record SearchResult(int TotalCount, List<Models.Device> Items);

    internal sealed class Handler(AppDbContext db)
        : IRequestHandler<Query, SearchResult>
    {
        public async Task<SearchResult> Handle(Query request, CancellationToken token = default)
        {
            var fields = DeviceSearchDataHelper.SearchCriteriaKeys;
            if (request.Fields?.Any() == true)
                fields = fields.Where(f => request.Fields.Contains(f.Index)).ToList();

            var deviceIds = DeviceSearchDataHelper.GetDeviceIds(db, fields, request.Filter ?? "");

            var query = db.Devices.AsNoTracking()
                .Where(d => deviceIds.Contains(d.Id))
                .Include(d => d.DeviceType)
                .Include(d => d.DeviceGroup)
                .Include(d => d.DeviceAddresses).ThenInclude(a => a.AddrType!);

            var totalCount = await query.CountAsync(token);
            var items = await query
                .OrderBy(d => d.Name)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync(token);

            return new SearchResult(totalCount, items);
        }
    }
}
