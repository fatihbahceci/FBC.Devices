using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.AddrType;

public sealed class AddrTypeCreate
{
    public record Command(string Name) : IRequest<int>;

    internal sealed class Handler(AddrTypeRepository repo)
        : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken token = default)
        {
            var entity = new Models.AddrType { Name = request.Name };
            await repo.ApplyOperation(EntityOperation.Create, entity, alsoValidate: false);
            return entity.Id;
        }
    }
}
