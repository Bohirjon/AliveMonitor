using AliveMonitor.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AliveMonitor.Infrastructure.Data.Configurations;

public class HealthCheckLogConfiguration : IEntityTypeConfiguration<HealthCheckLog>
{
    public void Configure(EntityTypeBuilder<HealthCheckLog> builder)
    {
        builder.ToTable("health_check_logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(l => l.CheckedAt).IsRequired();
        builder.Property(l => l.ResponseTimeMs).IsRequired();
        builder.Property(l => l.IsHealthy).IsRequired();
        builder.Property(l => l.ErrorMessage).HasMaxLength(2048);

        builder.HasOne(l => l.Endpoint)
            .WithMany(e => e.HealthCheckLogs)
            .HasForeignKey(l => l.EndpointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.EndpointId, l.CheckedAt });
    }
}
