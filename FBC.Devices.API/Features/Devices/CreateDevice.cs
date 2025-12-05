using FBC.Devices.API.MediatR;

namespace FBC.Devices.API.Features.Devices;

public sealed class CreateDevice
{
    public record Command(Models.DeviceRequestModel Device) : IRequest<long>;
    internal sealed class CreateDeviceHandler(ILogger<CreateDeviceHandler> logger) : IRequestHandler<Command, long>
    {

        public async Task<long> Handle(Command request, CancellationToken token = default)
        {
            logger.LogInformation("Creating device with Name: {DeviceName}", request.Device.Name);
            //return await _deviceService.CreateDeviceAsync(request.Device, token);
            return -1;
        }
    }
}
public sealed class CreateDeviceEndPoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/devices", async (CreateDevice.Command command, IMediator mediator, CancellationToken token) =>
        {
            var deviceId = await mediator.Send(command, token);
            //return Results.Created($"/devices/{deviceId}", new { DeviceId = deviceId });
            return Results.Created($"/devices/{deviceId}", deviceId);
        })
            .WithTags("Devices")
            .WithName("CreateDevice")
            .WithSummary("Creates a new device.")
            .WithDescription("Creates a new device and returns the ID of the created device.")
            .Produces<long>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}


public sealed class  VoidTest
{
    public record Command() : IRequest;
    internal sealed class VoidTestHandler(ILogger<VoidTestHandler> logger) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            logger.LogInformation("Handling void command");
            await Task.CompletedTask;
        }
    }

}
public sealed class VoidTestEndPoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/voidtest", async (VoidTest.Command command, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(command, token);
            return Results.Ok();
        })
            .WithTags("Test")
            .WithName("VoidTest")
            .WithSummary("Tests void command.")
            .WithDescription("Sends a void command and returns OK.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
