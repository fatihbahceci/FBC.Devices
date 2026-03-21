using FBC.Devices.API.Data;
using FBC.Devices.API.Data.Repositories;
using FBC.Devices.API.Models;
using FBC.Mediator;

namespace FBC.Devices.API.Features.User;

public sealed class UserCreate
{
    public record Command(string UserName, string Password, string Name, bool IsSysAdmin, string[] Roles) : IRequest<int>;

    internal sealed class Handler(UserRepository db)
        : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken token = default)
        {
            if (await db.AnyAsync(x => x.UserName == request.UserName))
                throw new ArgumentException($"User with username '{request.UserName}' already exists.");

            var user = new AppUser
            {
                UserName = request.UserName,
                NewPassword = request.Password,
                Name = request.Name,
                IsSysAdmin = request.IsSysAdmin
            };
            user.SetRoles(request.Roles);
            await db.ApplyOperation(DBRepository.EntityOperation.Create, user,true);
            return user.Id;
        }
    }
}
