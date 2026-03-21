using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.User;

public sealed class UserGetById
{
    public record Query(int Id) : IRequest<UserDto?>;

    public record UserDto(int Id, string UserName, string Name, bool IsSysAdmin, string[] Roles);

    internal sealed class Handler(UserRepository repo)
        : IRequestHandler<Query, UserDto?>
    {
        public async Task<UserDto?> Handle(Query request, CancellationToken token = default)
        {
            var user = await repo.GetByIdAsync(request.Id, enableTracking: false, cancellationToken: token);
            if (user is null) return null;
            return new UserDto(user.Id, user.UserName, user.Name, user.IsSysAdmin, user.GetRoles());
        }
    }
}
