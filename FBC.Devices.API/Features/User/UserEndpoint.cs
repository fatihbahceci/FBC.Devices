using FBC.Mediator;

namespace FBC.Devices.API.Features.User;

public sealed class UserEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization(Constants.UserRoles.SysAdmin);

        group.MapGet("/", async (int? pageNumber, int? itemsPerPage, IMediator mediator, CancellationToken token) =>
            Results.Ok(await mediator.Send(
                new UserGetAll.Query(pageNumber ?? 0, itemsPerPage ?? 25), token)));

        group.MapGet("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
            await mediator.Send(new UserGetById.Query(id), token)
                is { } user ? Results.Ok(user) : Results.NotFound());

        group.MapPost("/", async (UserCreate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            var id = await mediator.Send(cmd, token);
            return Results.Created($"/api/users/{id}", id);
        });

        group.MapPut("/{id}", async (int id, UserUpdate.Command cmd, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(cmd with { Id = id }, token);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, IMediator mediator, CancellationToken token) =>
        {
            await mediator.Send(new UserDelete.Command(id), token);
            return Results.NoContent();
        });
    }
}
