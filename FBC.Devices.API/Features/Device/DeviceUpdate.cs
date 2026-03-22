using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.Device;

public sealed class DeviceUpdate
{
    public record Command(
        int Id,
        string Name,
        string? Description,
        int? DeviceGroupId,
        int? DeviceTypeId,
        string? DeviceModel,
        string? SerialNumber,
        string? Location,
        string? Note,
        bool IsActive,
        List<DeviceAddrDto>? Addresses) : IRequest;

    public record DeviceAddrDto(int Id, int AddrTypeId, string? Addr, string? Username, string? Password, bool PeriodicPingCheck);

    internal sealed class Handler(DeviceRepository deviceRepo, DeviceAddrRepository deviceAddrRepo)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken token = default)
        {
            var device = await deviceRepo.GetByIdAsync(request.Id, cancellationToken: token)
                ?? throw new KeyNotFoundException($"Device {request.Id} not found");

            await deviceRepo.BeginTransactionAsync(token);
            try
            {
                device.Name = request.Name;
                device.Description = request.Description;
                device.DeviceGroupId = request.DeviceGroupId;
                device.DeviceTypeId = request.DeviceTypeId;
                device.DeviceModel = request.DeviceModel;
                device.SerialNumber = request.SerialNumber;
                device.Location = request.Location;
                device.Note = request.Note;
                device.IsActive = request.IsActive;
                await deviceRepo.ApplyOperation(EntityOperation.Update, device, alsoValidate: true);

                // Handle addresses
                if (request.Addresses != null)
                {
                    var existingAddrs = (await deviceAddrRepo.GetListAsync(a => a.DeviceId == request.Id, cancellationToken: token))?.Items
                        ?? new List<Models.DeviceAddr>();
                    var incomingIds = request.Addresses.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();

                    // Delete removed addresses
                    var toDelete = existingAddrs.Where(a => !incomingIds.Contains(a.Id)).ToList();
                    if (toDelete.Any())
                        await deviceAddrRepo.ApplyOperationRange(EntityOperation.Delete, toDelete, false, true);

                    // Update existing and add new
                    var toCreate = new List<Models.DeviceAddr>();
                    foreach (var addrDto in request.Addresses)
                    {
                        if (addrDto.Id > 0)
                        {
                            var existing = existingAddrs.FirstOrDefault(a => a.Id == addrDto.Id);
                            if (existing != null)
                            {
                                existing.AddrTypeId = addrDto.AddrTypeId;
                                existing.Addr = addrDto.Addr;
                                existing.Username = addrDto.Username;
                                existing.Password = addrDto.Password;
                                existing.PeriodicPingCheck = addrDto.PeriodicPingCheck;
                                await deviceAddrRepo.ApplyOperation(EntityOperation.Update, existing, alsoValidate: true);
                            }
                        }
                        else
                        {
                            toCreate.Add(new Models.DeviceAddr
                            {
                                DeviceId = request.Id,
                                AddrTypeId = addrDto.AddrTypeId,
                                Addr = addrDto.Addr,
                                Username = addrDto.Username,
                                Password = addrDto.Password,
                                PeriodicPingCheck = addrDto.PeriodicPingCheck
                            });
                        }
                    }

                    if (toCreate.Any())
                        await deviceAddrRepo.ApplyOperationRange(EntityOperation.Create, toCreate, alsoValidate: true);
                }

                await deviceRepo.CommitTransactionAsync(token);
            }
            catch
            {
                await deviceRepo.RollbackTransactionAsync(token);
                throw;
            }
        }
    }
}
