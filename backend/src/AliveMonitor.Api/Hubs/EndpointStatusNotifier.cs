using AliveMonitor.Core.Enums;
using AliveMonitor.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AliveMonitor.Api.Hubs;

public class EndpointStatusNotifier(IHubContext<EndpointStatusHub> hubContext) : IEndpointStatusNotifier
{
    public async Task NotifyStatusChangedAsync(Guid userId, Guid endpointId, EndpointStatus newStatus, DateTime? lastCheckedAt)
    {
        await hubContext.Clients.Group(userId.ToString()).SendAsync("EndpointStatusChanged", new
        {
            EndpointId = endpointId,
            Status = newStatus.ToString(),
            LastCheckedAt = lastCheckedAt,
        });
    }
}
