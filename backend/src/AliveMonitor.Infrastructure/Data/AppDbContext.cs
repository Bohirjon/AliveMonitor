using AliveMonitor.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AliveMonitor.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<MonitoredEndpoint> MonitoredEndpoints => Set<MonitoredEndpoint>();
    public DbSet<HealthCheckLog> HealthCheckLogs => Set<HealthCheckLog>();
    public DbSet<Incident> Incidents => Set<Incident>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
