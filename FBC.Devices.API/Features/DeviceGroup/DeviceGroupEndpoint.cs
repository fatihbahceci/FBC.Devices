using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceGroup;

public sealed class DeviceGroupEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/device-groups")
            .WithTags("Device Groups")
            .RequireAuthorization(Constants.UserRoles.EditDeviceGroups);

        group.MapGet("/", async (IMediator mediator, CancellationToken token) =>
            Results.Ok(await mediator.Send(new DeviceGroupGetAll.Query(), token)));

        group.MapGet("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
            await mediator.Send(new DeviceGroupGetById.Query(id), token)
                is { } item ? Results.Ok(item) : Results.NotFound());

        group.MapPost("/", async (DeviceGroupCreate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            var id = await mediator.Send(cmd, token);
            return Results.Created($"/api/device-groups/{id}", id);
        });

        group.MapPut("/{id}", async (int id, DeviceGroupUpdate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(cmd with { Id = id }, token);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(new DeviceGroupDelete.Command(id), token);
            return Results.NoContent();
        });
    }
}
