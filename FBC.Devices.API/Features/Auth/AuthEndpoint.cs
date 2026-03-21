using FBC.Mediator;

namespace FBC.Devices.API.Features.Auth;

public sealed class AuthEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").AllowAnonymous();

        group.MapPost("/login", async (AuthLogin.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            var result = await mediator.Send(cmd, token);
            return result is not null ? Results.Ok(result) : Results.Unauthorized();
        });
    }
}
