using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.User;

public sealed class UserDelete
{
    public record Command(int Id) : IRequest;

    internal sealed class Handler(UserRepository repo)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var user = await repo.GetByIdAsync(request.Id, cancellationToken: token);
            if (user is null) return;

            await repo.ApplyOperation(EntityOperation.Delete, user, alsoValidate: true, deletePermanent: true);
        }
    }
}
