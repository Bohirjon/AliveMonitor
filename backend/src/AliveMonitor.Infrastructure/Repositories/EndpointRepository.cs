using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Enums;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AliveMonitor.Infrastructure.Repositories;

public class EndpointRepository(AppDbContext db) : IEndpointRepository
{
    public async Task<MonitoredEndpoint?> GetByIdAsync(Guid id, Guid userId)
        => await db.MonitoredEndpoints
            .Include(e => e.Team)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

    public async Task<List<MonitoredEndpoint>> GetAllAsync(Guid userId, string? search = null, EndpointStatus? status = null)
    {
        var query = db.MonitoredEndpoints.Include(e => e.Team).Where(e => e.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(e => e.FriendlyName.ToLower().Contains(term) || e.Url.ToLower().Contains(term));
        }

        if (status is not null)
        {
            query = query.Where(e => e.CurrentStatus == status);
        }

        return await query.OrderBy(e => e.FriendlyName).ToListAsync();
    }

    public async Task<MonitoredEndpoint> CreateAsync(MonitoredEndpoint endpoint)
    {
        db.MonitoredEndpoints.Add(endpoint);
        await db.SaveChangesAsync();
        return endpoint;
    }

    public async Task UpdateAsync(MonitoredEndpoint endpoint)
    {
        endpoint.UpdatedAt = DateTime.UtcNow;
        db.MonitoredEndpoints.Update(endpoint);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(MonitoredEndpoint endpoint)
    {
        db.MonitoredEndpoints.Remove(endpoint);
        await db.SaveChangesAsync();
    }

    public async Task<List<MonitoredEndpoint>> GetAllEnabledAsync()
        => await db.MonitoredEndpoints
            .Where(e => e.IsEnabled)
            .ToListAsync();
}
