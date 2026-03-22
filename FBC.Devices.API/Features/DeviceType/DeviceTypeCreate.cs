using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceType;

public sealed class DeviceTypeCreate
{
    public record Command(string Name, string? Description) : IRequest<int>;

    internal sealed class Handler(DeviceTypeRepository repo)
        : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken token = default)
        {
            var entity = new Models.DeviceType { Name = request.Name, Description = request.Description };
            await repo.ApplyOperation(EntityOperation.Create, entity, alsoValidate: true);
            return entity.Id;
        }
    }
}
