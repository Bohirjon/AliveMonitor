using AliveMonitor.Core.Entities;
using AliveMonitor.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AliveMonitor.Infrastructure.Data.Configurations;

public class MonitoredEndpointConfiguration : IEntityTypeConfiguration<MonitoredEndpoint>
{
    public void Configure(EntityTypeBuilder<MonitoredEndpoint> builder)
    {
        builder.ToTable("monitored_endpoints");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.FriendlyName).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Url).IsRequired().HasMaxLength(2048);

        builder.Property(e => e.IntervalMinutes).HasDefaultValue(1);
        builder.Property(e => e.TimeoutSeconds).HasDefaultValue(30);
        builder.Property(e => e.IsEnabled).HasDefaultValue(false);

        builder.Property(e => e.CustomHeadersJson).HasColumnType("jsonb");

        builder.Property(e => e.ExpectedStatusCode).HasDefaultValue(200);
        builder.Property(e => e.JsonPropertyName).HasMaxLength(256);
        builder.Property(e => e.JsonPropertyExpectedValue).HasMaxLength(1024);

        builder.Property(e => e.CurrentStatus)
            .HasDefaultValue(EndpointStatus.Disabled)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(e => e.User)
            .WithMany(u => u.MonitoredEndpoints)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Team)
            .WithMany(t => t.MonitoredEndpoints)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.IsEnabled });
    }
}
