using FBC.DBRepository;
using FBC.Devices.API.Data;
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
                foreach (var addrDto in request.Addresses)
                {
                    var addr = new Models.DeviceAddr
                    {
                        DeviceId = device.Id,
                        AddrTypeId = addrDto.AddrTypeId,
                        Addr = addrDto.Addr,
                        Username = addrDto.Username,
                        Password = addrDto.Password,
                        PeriodicPingCheck = addrDto.PeriodicPingCheck
                    };
                    deviceAddrRepo.ApplyOperation(EntityOperation.Create, addr, alsoValidate: true).GetAwaiter().GetResult();
                }
            }

            return device.Id;
        }
    }
}
