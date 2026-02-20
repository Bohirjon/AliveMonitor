using AliveMonitor.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AliveMonitor.Infrastructure.Data.Configurations;

public class SslCertificateCheckLogConfiguration : IEntityTypeConfiguration<SslCertificateCheckLog>
{
    public void Configure(EntityTypeBuilder<SslCertificateCheckLog> builder)
    {
        builder.ToTable("ssl_certificate_check_logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(l => l.CheckedAt).IsRequired();
        builder.Property(l => l.IsValid).IsRequired();
        builder.Property(l => l.SubjectName).HasMaxLength(512);
        builder.Property(l => l.IssuerName).HasMaxLength(512);
        builder.Property(l => l.ErrorMessage).HasMaxLength(2048);

        builder.HasOne(l => l.Endpoint)
            .WithMany(e => e.SslCertificateCheckLogs)
            .HasForeignKey(l => l.EndpointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.EndpointId, l.CheckedAt });
    }
}
