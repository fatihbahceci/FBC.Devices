using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Devices.API.Models;
using FBC.Mediator;

namespace FBC.Devices.API.Features.User;

public sealed class UserCreate
{
    public record Command(string UserName, string Password, string Name, bool IsSysAdmin, string[] Roles) : IRequest<int>;

    internal sealed class Handler(UserRepository repo)
        : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken token = default)
        {
            var user = new AppUser
            {
                UserName = request.UserName,
                NewPassword = request.Password,
                Name = request.Name,
                IsSysAdmin = request.IsSysAdmin
            };
            user.SetRoles(request.Roles);
            await repo.ApplyOperation(EntityOperation.Create, user, alsoValidate: true);
            return user.Id;
        }
    }
}
