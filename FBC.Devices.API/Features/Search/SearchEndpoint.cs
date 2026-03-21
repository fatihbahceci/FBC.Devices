using FBC.Mediator;

namespace FBC.Devices.API.Features.Search;

public sealed class SearchEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/search")
            .WithTags("Search")
            .RequireAuthorization(Constants.UserRoles.ViewDevices);

        group.MapGet("/devices", async (string? filter, string? fields, int? skip, int? take,
            IMediator mediator, CancellationToken token) =>
        {
            int[]? fieldIndexes = null;
            if (!string.IsNullOrEmpty(fields))
                fieldIndexes = fields.Split(',').Select(int.Parse).ToArray();

            var result = await mediator.Send(
                new SearchDevices.Query(filter, fieldIndexes, skip ?? 0, take ?? 10), token);
            return Results.Ok(result);
        });

        group.MapGet("/criteria", async (IMediator mediator, CancellationToken token) =>
            Results.Ok(await mediator.Send(new SearchGetCriteria.Query(), token)));
    }
}
