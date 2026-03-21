using System.Net.NetworkInformation;
using FBC.Mediator;

namespace FBC.Devices.API.Features.Status;

public sealed class StatusPing
{
    public record Command(string Address) : IRequest<PingResultDto>;

    public record PingResultDto(bool Success, string Status, long RoundtripTime);

    internal sealed class Handler
        : IRequestHandler<Command, PingResultDto>
    {
        public Task<PingResultDto> Handle(Command request, CancellationToken token = default)
        {
            try
            {
                var ping = new Ping();
                var reply = ping.Send(request.Address, 5000);
                return Task.FromResult(new PingResultDto(
                    reply.Status == IPStatus.Success,
                    reply.Status.ToString(),
                    reply.RoundtripTime));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new PingResultDto(false, ex.Message, 0));
            }
        }
    }
}
