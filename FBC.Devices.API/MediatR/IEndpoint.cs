namespace FBC.Devices.API.MediatR;

public interface IEndpoint
{
    void AddRoutes(IEndpointRouteBuilder app);
}
