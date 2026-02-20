using AliveMonitor.Core.Enums;

namespace AliveMonitor.Core.Interfaces;

public interface IEndpointStatusNotifier
{
    Task NotifyStatusChangedAsync(Guid userId, Guid endpointId, EndpointStatus newStatus, DateTime? lastCheckedAt);
}
