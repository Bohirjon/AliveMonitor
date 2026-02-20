using AliveMonitor.Core.Entities;

namespace AliveMonitor.Core.Interfaces;

public interface IHealthCheckLogRepository
{
    Task CreateAsync(HealthCheckLog log);
    Task<List<HealthCheckLog>> GetByEndpointAsync(Guid endpointId, DateTime from, DateTime to, int page, int pageSize);
    Task<int> GetCountAsync(Guid endpointId, DateTime from, DateTime to);
    Task<List<HealthCheckLog>> GetForAnalyticsAsync(Guid endpointId, DateTime from, DateTime to);
}
