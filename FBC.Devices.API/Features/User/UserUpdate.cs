using FBC.Devices.API.Data;
using FBC.Mediator;

namespace FBC.Devices.API.Features.User;

public sealed class UserUpdate
{
    public record Command(int Id, string UserName, string? NewPassword, string Name, bool IsSysAdmin, string[] Roles) : IRequest;

    internal sealed class Handler(AppDbContext db)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var user = db.SysUsers.FirstOrDefault(x => x.Id == request.Id)
                ?? throw new KeyNotFoundException($"User {request.Id} not found");

            if (db.SysUsers.Any(x => x.UserName == request.UserName && x.Id != request.Id))
                throw new ArgumentException($"User with username '{request.UserName}' already exists.");

            if (user.IsSysAdmin && !request.IsSysAdmin && db.SysUsers.Count(x => x.IsSysAdmin) <= 1)
                throw new InvalidOperationException("Cannot remove SysAdmin rights from the last SysAdmin user.");

            user.UserName = request.UserName;
            user.Name = request.Name;
            user.IsSysAdmin = request.IsSysAdmin;
            user.SetRoles(request.Roles);

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
                user.NewPassword = request.NewPassword;

            user.AdjustData(true);
            await db.SaveChangesAsync(token);
        }
    }
}
