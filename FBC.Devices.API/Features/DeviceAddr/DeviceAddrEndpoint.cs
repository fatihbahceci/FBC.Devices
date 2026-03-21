using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceAddr;

public sealed class DeviceAddrEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/device-addresses")
            .WithTags("Device Addresses")
            .RequireAuthorization(Constants.UserRoles.EditDevices);

        group.MapGet("/by-device/{deviceId}", async (int deviceId, IMediator mediator, CancellationToken token) =>
            Results.Ok(await mediator.Send(new DeviceAddrGetByDeviceId.Query(deviceId), token)));

        group.MapPost("/", async (DeviceAddrCreate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            var id = await mediator.Send(cmd, token);
            return Results.Created($"/api/device-addresses/{id}", id);
        });

        group.MapPut("/{id}", async (int id, DeviceAddrUpdate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(cmd with { Id = id }, token);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(new DeviceAddrDelete.Command(id), token);
            return Results.NoContent();
        });
    }
}
