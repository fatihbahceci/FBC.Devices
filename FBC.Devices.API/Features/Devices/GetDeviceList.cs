using FBC.Devices.API.DBModels.Repository;
using FBC.Devices.API.Features.Devices.Models;
using FBC.Mediator;
using FBC.Devices.DBModels;
using Microsoft.AspNetCore.Mvc;
using FBC.DBRepository;

namespace FBC.Devices.API.Features.Devices;
/*
 TODO: 1. as you can see there are wrong values for count and pages in the example below. Also hasNext is true even though there is only one item.

{
    "size": 0,
    "index": 0,
    "count": 1,
    "pages": 2147483647,
    "items": [
        {
            "name": "string",
            "description": "string",
            "deviceGroupId": null,
            "deviceGroup": null,
            "deviceTypeId": null,
            "deviceType": null,
            "deviceModel": "string",
            "serialNumber": "string",
            "location": "string",
            "note": "string",
            "isActive": true,
            "deviceAddresses": [],
            "isDeleted": false,
            "createdDateUTC": "2025-12-06T10:42:27.217313Z",
            "updatedDateUTC": null,
            "deletedDateUTC": null,
            "id": 1
        }
    ],
    "hasPrevius": false,
    "hasNext": true
}

   2. Add pagination and filter parameters to the command and implement pagination in the handler
   3. Probably we will delete this endpoint and use a more generic search endpoint instead.
 */
public sealed class GetDeviceList
{
    public record Command(DynamicQuery? query, bool RetrieveEvenMarkedDeleted = false) : IRequest<PaginateResponseModel<Device>>;
    internal sealed class GetDeviceListHandler(ILogger<GetDeviceListHandler> logger, IDeviceRepository repo) : IRequestHandler<Command, PaginateResponseModel<Device>>
    {

        public async Task<PaginateResponseModel<Device>> Handle(Command request, CancellationToken token = default)
        {
            logger.LogInformation("Get devices");
            return
                request.query is not null
                ? await repo.GetListAsync(repo.GetQueryable().ToDynamic(request.query), includeDeletedRecords: request.RetrieveEvenMarkedDeleted)
                : await repo.GetListAsync(includeDeletedRecords: request.RetrieveEvenMarkedDeleted);
        }
    }
}
public sealed class GetDeviceListEndPoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        //Use MapPost if you want to pass complex object in body
        app.MapPost("/devices/GetDeviceList", async (GetDeviceList.Command command, IMediator mediator, CancellationToken token) =>
        {
            //var device = await mediator.Send(command, token);
            var device = await mediator.Send(command, token);
            return Results.Ok(device);
        })
            .WithTags("Devices")
            .WithName("GetDeviceList")
            .WithSummary("Get Device List")
            .WithDescription("Retrieves the list of devices.")
            .Produces<PaginateResponseModel<Device>>(StatusCodes.Status200OK)
            .Produces<DynamicQuery>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
