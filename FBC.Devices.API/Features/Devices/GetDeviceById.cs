using FBC.Devices.API.DBModels.Repository;
using FBC.Devices.API.Features.Devices.Models;
using FBC.Devices.API.MediatR;
using FBC.Devices.DBModels;
using Microsoft.AspNetCore.Mvc;

namespace FBC.Devices.API.Features.Devices;

public sealed class GetDeviceById
{
    public record Command(long DeviceId, bool RetrieveEvenMarkedDeleted = false) : IRequest<Device>;
    internal sealed class GetDeviceByIdHandler(ILogger<GetDeviceByIdHandler> logger, IDeviceRepository repo) : IRequestHandler<Command, Device>
    {

        public async Task<Device> Handle(Command request, CancellationToken token = default)
        {
            logger.LogInformation("Get device with Id: {DeviceId}", request.DeviceId);
            var exists = await repo.GetAsync(x => x.Id == request.DeviceId && (request.RetrieveEvenMarkedDeleted || !x.IsDeleted));
            if (exists == null)
            {
                throw new NotFoundException($"Device with Id {request.DeviceId} not found.");
            }
            return exists;
        }
    }
}
public sealed class GetDeviceByIdEndPoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        //Use MapPost if you want to pass complex object in body
        app.MapGet("/devices/getDeviceById/{deviceId}", async (long id, bool RetrieveEvenMarkedDeleted /*GetDeviceById.Command command*/, IMediator mediator, CancellationToken token) =>
        {
            //var device = await mediator.Send(command, token);
            var device = await mediator.Send(new GetDeviceById.Command(id, RetrieveEvenMarkedDeleted), token);
            return Results.Ok(device);
        })
            .WithTags("Devices")
            .WithName("GetDeviceById")
            .WithSummary("Gets a device by its ID.")
            .WithDescription("Retrieves a device from the database using the provided device ID.")
            .Produces<Device>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
