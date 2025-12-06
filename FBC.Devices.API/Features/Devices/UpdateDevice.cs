using FBC.Devices.API.DBModels.Repository;
using FBC.Devices.API.Features.Devices.Models;
using FBC.Mediator;
using FBC.Devices.DBModels;

namespace FBC.Devices.API.Features.Devices;

public sealed class UpdateDevice
{
    public record Command(long DeviceId, DeviceRequestModel Device) : IRequest<long>;
    internal sealed class UpdateDeviceHandler(ILogger<UpdateDeviceHandler> logger, IDeviceRepository repo) : IRequestHandler<Command, long>
    {

        public async Task<long> Handle(Command request, CancellationToken token = default)
        {
            logger.LogInformation("Updating device with Id: {DeviceId}", request.DeviceId);
            var exists = await repo.GetAsync(x => x.Id == request.DeviceId);
            if (exists == null)
            {
                throw new NotFoundException($"Device with Id {request.DeviceId} not found.");
            }
            new DeviceMapper().UpdateDeviceFromModel(request.Device, exists);
            var r = await repo.ApplyOperation(EntityOperation.Update, exists, true);
            return r.Id;
        }
    }
}
public sealed class UpdateDeviceEndPoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/devices/update", async (UpdateDevice.Command command, IMediator mediator, CancellationToken token) =>
        {
            var deviceId = await mediator.Send(command, token);
            //return Results.Created($"/devices/{deviceId}", new { DeviceId = deviceId });
            return Results.Ok(deviceId);
        })
            .WithTags("Devices")
            .WithName("UpdateDevice")
            .WithSummary("Updates an existing device.")
            .WithDescription("Updates an existing device in the system with the provided details.")
            .Produces<long>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
