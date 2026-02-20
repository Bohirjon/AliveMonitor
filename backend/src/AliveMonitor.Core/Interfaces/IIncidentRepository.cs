using AliveMonitor.Core.Entities;

namespace AliveMonitor.Core.Interfaces;

public interface IIncidentRepository
{
    Task<Incident?> GetOpenIncidentAsync(Guid endpointId);
    Task<List<Incident>> GetByEndpointAsync(Guid endpointId, DateTime from, DateTime to);
    Task CreateAsync(Incident incident);
    Task UpdateAsync(Incident incident);
}
