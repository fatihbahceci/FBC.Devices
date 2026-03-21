using FBC.Devices.API.Services;
using FBC.Mediator;

namespace FBC.Devices.API.Features.Search;

public sealed class SearchGetCriteria
{
    public record Query() : IRequest<List<SearchCriteriaDto>>;

    public record SearchCriteriaDto(int Index, string Table, string FieldName);

    internal sealed class Handler
        : IRequestHandler<Query, List<SearchCriteriaDto>>
    {
        public Task<List<SearchCriteriaDto>> Handle(Query request, CancellationToken token = default)
        {
            var result = DeviceSearchDataHelper.SearchCriteriaKeys
                .Select(k => new SearchCriteriaDto(k.Index, k.Table.ToString(), k.FieldName))
                .ToList();
            return Task.FromResult(result);
        }
    }
}
