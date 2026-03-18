using AliveMonitor.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AliveMonitor.Infrastructure.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.Name).IsRequired().HasMaxLength(256);

        builder.Property(t => t.MemberEmails)
            .HasColumnType("jsonb");

        builder.Property(t => t.TelegramChatId);

        builder.Property(t => t.WebhookUrl).HasMaxLength(2048);

        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(t => t.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(t => t.User)
            .WithMany(u => u.Teams)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.UserId);
    }
}
