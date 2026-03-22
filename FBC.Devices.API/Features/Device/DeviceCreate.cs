using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.Device;

public sealed class DeviceCreate
{
    public record Command(
        string Name,
        string? Description,
        int? DeviceGroupId,
        int? DeviceTypeId,
        string? DeviceModel,
        string? SerialNumber,
        string? Location,
        string? Note,
        bool IsActive,
        List<DeviceAddrDto>? Addresses) : IRequest<int>;

    public record DeviceAddrDto(int AddrTypeId, string? Addr, string? Username, string? Password, bool PeriodicPingCheck);

    internal sealed class Handler(DeviceRepository deviceRepo, DeviceAddrRepository deviceAddrRepo)
        : IRequestHandler<Command, int>
    {
        public async Task<int> Handle(Command request, CancellationToken token = default)
        {
            await deviceRepo.BeginTransactionAsync(token);
            try
            {
                var device = new Models.Device
                {
                    Name = request.Name,
                    Description = request.Description,
                    DeviceGroupId = request.DeviceGroupId,
                    DeviceTypeId = request.DeviceTypeId,
                    DeviceModel = request.DeviceModel,
                    SerialNumber = request.SerialNumber,
                    Location = request.Location,
                    Note = request.Note,
                    IsActive = request.IsActive
                };
                await deviceRepo.ApplyOperation(EntityOperation.Create, device, alsoValidate: true);

                if (request.Addresses?.Any() == true)
                {
                    var addresses = request.Addresses.Select(a => new Models.DeviceAddr
                    {
                        DeviceId = device.Id,
                        AddrTypeId = a.AddrTypeId,
                        Addr = a.Addr,
                        Username = a.Username,
                        Password = a.Password,
                        PeriodicPingCheck = a.PeriodicPingCheck
                    }).ToList();

                    await deviceAddrRepo.ApplyOperationRange(EntityOperation.Create, addresses, alsoValidate: true);
                }

                await deviceRepo.CommitTransactionAsync(token);
                return device.Id;
            }
            catch
            {
                await deviceRepo.RollbackTransactionAsync(token);
                throw;
            }
        }
    }
}
