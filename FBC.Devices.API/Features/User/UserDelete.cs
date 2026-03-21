using FBC.Devices.API.Data;
using FBC.Mediator;

namespace FBC.Devices.API.Features.User;

public sealed class UserDelete
{
    public record Command(int Id) : IRequest;

    internal sealed class Handler(AppDbContext db)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var user = db.SysUsers.FirstOrDefault(x => x.Id == request.Id);
            if (user is null) return;

            if (user.IsSysAdmin && db.SysUsers.Count(x => x.IsSysAdmin) <= 1)
                throw new InvalidOperationException("Cannot delete the last SysAdmin user.");

            db.SysUsers.Remove(user);
            await db.SaveChangesAsync(token);
        }
    }
}
