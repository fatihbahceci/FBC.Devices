using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.DeviceType;

public sealed class DeviceTypeUpdate
{
    public record Command(int Id, string Name, string? Description) : IRequest;

    internal sealed class Handler(DeviceTypeRepository repo)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken: token)
                ?? throw new KeyNotFoundException($"DeviceType {request.Id} not found");
            entity.Name = request.Name;
            entity.Description = request.Description;
            await repo.ApplyOperation(EntityOperation.Update, entity, alsoValidate: false);
        }
    }
}
