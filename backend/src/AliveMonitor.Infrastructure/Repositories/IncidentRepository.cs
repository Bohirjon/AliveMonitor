using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AliveMonitor.Infrastructure.Repositories;

public class IncidentRepository(AppDbContext db) : IIncidentRepository
{
    public async Task<Incident?> GetOpenIncidentAsync(Guid endpointId)
        => await db.Incidents
            .FirstOrDefaultAsync(i => i.EndpointId == endpointId && i.ResolvedAt == null);

    public async Task<List<Incident>> GetByEndpointAsync(Guid endpointId, DateTime from, DateTime to)
        => await db.Incidents
            .Where(i => i.EndpointId == endpointId && i.OpenedAt >= from && i.OpenedAt <= to)
            .OrderByDescending(i => i.OpenedAt)
            .ToListAsync();

    public async Task CreateAsync(Incident incident)
    {
        db.Incidents.Add(incident);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Incident incident)
    {
        db.Incidents.Update(incident);
        await db.SaveChangesAsync();
    }
}
