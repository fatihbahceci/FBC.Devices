using FBC.Devices.API.DBModels.Repository;
using FBC.Devices.API.Features.Devices.Models;
using FBC.Mediator;
using FBC.Devices.DBModels;
using Microsoft.AspNetCore.Mvc;

namespace FBC.Devices.API.Features.Devices;

public sealed class DeleteDevice
{
    public record Command(long DeviceId, bool DeletePermanently = false) : IRequest<long>;
    internal sealed class DeleteDeviceHandler(ILogger<DeleteDeviceHandler> logger, IDeviceRepository repo) : IRequestHandler<Command, long>
    {

        public async Task<long> Handle(Command request, CancellationToken token = default)
        {
            logger.LogInformation("Deleting device with Id: {DeviceId}", request.DeviceId);
            var exists = await repo.GetAsync(x => x.Id == request.DeviceId);
            //TODO I'm not sure throwing KeyNotFoundException
            if (exists == null)
            {
                throw new NotFoundException($"Device with Id {request.DeviceId} not found.");
            }
            if (exists.IsDeleted && request.DeletePermanently == false)
            {
                logger.LogWarning("Device with Id: {DeviceId} is already deleted.", request.DeviceId);
                return exists.Id;
            }
            var r = await repo.ApplyOperation(EntityOperation.Delete, exists, true, request.DeletePermanently);
            return r.Id;
        }
    }
}
public sealed class DeleteDeviceEndPoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/devices/delete/{command}", async ([FromBody] DeleteDevice.Command command, IMediator mediator, CancellationToken token) =>
        {
            var deviceId = await mediator.Send(command, token);
            //return Results.Created($"/devices/{deviceId}", new { DeviceId = deviceId });
            return Results.Ok(deviceId);
        })
            .WithTags("Devices")
            .WithName("DeleteDevice")
            .WithSummary("Deletes an existing device.")
            .WithDescription("Deletes an existing device. Can be a soft delete or permanent delete based on the request.")
            .Produces<long>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
