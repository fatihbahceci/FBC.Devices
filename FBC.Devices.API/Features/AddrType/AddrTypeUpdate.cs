using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.AddrType;

public sealed class AddrTypeUpdate
{
    public record Command(int Id, string Name) : IRequest;

    internal sealed class Handler(AddrTypeRepository repo)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken: token)
                ?? throw new KeyNotFoundException($"AddrType {request.Id} not found");
            entity.Name = request.Name;
            await repo.ApplyOperation(EntityOperation.Update, entity, alsoValidate: false);
        }
    }
}
