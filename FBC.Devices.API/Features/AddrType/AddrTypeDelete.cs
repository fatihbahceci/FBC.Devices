using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.AddrType;

public sealed class AddrTypeDelete
{
    public record Command(int Id) : IRequest;

    internal sealed class Handler(AddrTypeRepository repo)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken: token);
            if (entity is null) return;
            await repo.ApplyOperation(EntityOperation.Delete, entity, alsoValidate: false, deletePermanent: true);
        }
    }
}
