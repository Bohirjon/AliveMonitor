using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Interfaces;
using AliveMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AliveMonitor.Infrastructure.Repositories;

public class HealthCheckLogRepository(AppDbContext db) : IHealthCheckLogRepository
{
    public async Task CreateAsync(HealthCheckLog log)
    {
        db.HealthCheckLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<List<HealthCheckLog>> GetByEndpointAsync(Guid endpointId, DateTime from, DateTime to, int page, int pageSize)
        => await db.HealthCheckLogs
            .Where(l => l.EndpointId == endpointId && l.CheckedAt >= from && l.CheckedAt <= to)
            .OrderByDescending(l => l.CheckedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetCountAsync(Guid endpointId, DateTime from, DateTime to)
        => await db.HealthCheckLogs
            .CountAsync(l => l.EndpointId == endpointId && l.CheckedAt >= from && l.CheckedAt <= to);

    public async Task<List<HealthCheckLog>> GetForAnalyticsAsync(Guid endpointId, DateTime from, DateTime to)
        => await db.HealthCheckLogs
            .Where(l => l.EndpointId == endpointId && l.CheckedAt >= from && l.CheckedAt <= to)
            .OrderBy(l => l.CheckedAt)
            .ToListAsync();
}
