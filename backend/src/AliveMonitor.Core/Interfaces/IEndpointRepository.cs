using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Enums;

namespace AliveMonitor.Core.Interfaces;

public interface IEndpointRepository
{
    Task<MonitoredEndpoint?> GetByIdAsync(Guid id, Guid userId);
    Task<List<MonitoredEndpoint>> GetAllAsync(Guid userId, string? search = null, EndpointStatus? status = null);
    Task<MonitoredEndpoint> CreateAsync(MonitoredEndpoint endpoint);
    Task UpdateAsync(MonitoredEndpoint endpoint);
    Task DeleteAsync(MonitoredEndpoint endpoint);
    Task<List<MonitoredEndpoint>> GetAllEnabledAsync();
}
