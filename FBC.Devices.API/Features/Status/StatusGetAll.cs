using FBC.Devices.API.Services;
using FBC.Mediator;

namespace FBC.Devices.API.Features.Status;

public sealed class StatusGetAll
{
    public record Query() : IRequest<Dictionary<int, DeviceAddressStatusDto>>;

    public record DeviceAddressStatusDto(DateTime LastPingTime, DateTime LastSuccessPingTime, bool IsSuccess);

    internal sealed class Handler
        : IRequestHandler<Query, Dictionary<int, DeviceAddressStatusDto>>
    {
        public Task<Dictionary<int, DeviceAddressStatusDto>> Handle(Query request, CancellationToken token = default)
        {
            var result = DeviceStatusService.GetAllStatuses()
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => new DeviceAddressStatusDto(
                        kvp.Value.LastPingTime,
                        kvp.Value.LastSuccessPingTime,
                        kvp.Value.IsSuccess));
            return Task.FromResult(result);
        }
    }
}
