using FBC.Mediator;

namespace FBC.Devices.API.Features.Status;

public sealed class StatusEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/status")
            .WithTags("Status")
            .RequireAuthorization(Constants.UserRoles.ViewDevices);

        group.MapGet("/ping-results", async (IMediator mediator, CancellationToken token) =>
            Results.Ok(await mediator.Send(new StatusGetAll.Query(), token)));

        group.MapPost("/ping", async (StatusPing.Command cmd, IMediator mediator, CancellationToken token) =>
            Results.Ok(await mediator.Send(cmd, token)));
    }
}
