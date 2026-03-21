using FBC.Mediator;

namespace FBC.Devices.API.Features.AddrType;

public sealed class AddrTypeEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/addr-types")
            .WithTags("Address Types")
            .RequireAuthorization(Constants.UserRoles.EditDeviceAddrTypes);

        group.MapGet("/", async (IMediator mediator, CancellationToken token) =>
            Results.Ok(await mediator.Send(new AddrTypeGetAll.Query(), token)));

        group.MapGet("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
            await mediator.Send(new AddrTypeGetById.Query(id), token)
                is { } item ? Results.Ok(item) : Results.NotFound());

        group.MapPost("/", async (AddrTypeCreate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            var id = await mediator.Send(cmd, token);
            return Results.Created($"/api/addr-types/{id}", id);
        });

        group.MapPut("/{id}", async (int id, AddrTypeUpdate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(cmd with { Id = id }, token);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(new AddrTypeDelete.Command(id), token);
            return Results.NoContent();
        });
    }
}
