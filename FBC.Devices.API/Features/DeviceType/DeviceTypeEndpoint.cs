using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceType;

public sealed class DeviceTypeEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/device-types")
            .WithTags("Device Types")
            .RequireAuthorization(Constants.UserRoles.EditDeviceTypes);

        group.MapGet("/", async (IMediator mediator, CancellationToken token) =>
            Results.Ok(await mediator.Send(new DeviceTypeGetAll.Query(), token)));

        group.MapGet("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
            await mediator.Send(new DeviceTypeGetById.Query(id), token)
                is { } item ? Results.Ok(item) : Results.NotFound());

        group.MapPost("/", async (DeviceTypeCreate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            var id = await mediator.Send(cmd, token);
            return Results.Created($"/api/device-types/{id}", id);
        });

        group.MapPut("/{id}", async (int id, DeviceTypeUpdate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(cmd with { Id = id }, token);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(new DeviceTypeDelete.Command(id), token);
            return Results.NoContent();
        });
    }
}
