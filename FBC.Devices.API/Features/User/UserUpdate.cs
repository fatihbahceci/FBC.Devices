using FBC.DBRepository;
using FBC.Devices.API.Data;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.User;

public sealed class UserUpdate
{
    public record Command(int Id, string UserName, string? NewPassword, string Name, bool IsSysAdmin, string[] Roles) : IRequest;

    internal sealed class Handler(UserRepository repo)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var user = await repo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"User {request.Id} not found");

            if (user.IsSysAdmin && !request.IsSysAdmin && await repo.CountAsync(x => x.IsSysAdmin) <= 1)
                throw new InvalidOperationException("Cannot remove SysAdmin rights from the last SysAdmin user.");

            user.UserName = request.UserName;
            user.Name = request.Name;
            user.IsSysAdmin = request.IsSysAdmin;
            user.SetRoles(request.Roles);

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
                user.NewPassword = request.NewPassword;

            await repo.ApplyOperation(EntityOperation.Update, user, alsoValidate: true);
        }
    }
}
