using AliveMonitor.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AliveMonitor.Infrastructure.Data.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.OpenedAt).IsRequired();
        builder.Property(i => i.LastNotifiedAt).IsRequired();
        builder.Property(i => i.FailureCount).HasDefaultValue(1);

        builder.HasOne(i => i.Endpoint)
            .WithMany(e => e.Incidents)
            .HasForeignKey(i => i.EndpointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => new { i.EndpointId, i.ResolvedAt });
    }
}
