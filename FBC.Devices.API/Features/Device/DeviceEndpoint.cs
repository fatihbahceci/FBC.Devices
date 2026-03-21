using FBC.Mediator;

namespace FBC.Devices.API.Features.Device;

public sealed class DeviceEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/devices")
            .WithTags("Devices")
            .RequireAuthorization(Constants.UserRoles.EditDevices);

        group.MapGet("/", async (int? pageNumber, int? itemsPerPage, IMediator mediator, CancellationToken token) =>
            Results.Ok(await mediator.Send(
                new DeviceGetAll.Query(pageNumber ?? 0, itemsPerPage ?? 25), token)));

        group.MapGet("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
            await mediator.Send(new DeviceGetById.Query(id), token)
                is { } device ? Results.Ok(device) : Results.NotFound());

        group.MapPost("/", async (DeviceCreate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            var id = await mediator.Send(cmd, token);
            return Results.Created($"/api/devices/{id}", id);
        });

        group.MapPut("/{id}", async (int id, DeviceUpdate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(cmd with { Id = id }, token);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(new DeviceDelete.Command(id), token);
            return Results.NoContent();
        });
    }
}
