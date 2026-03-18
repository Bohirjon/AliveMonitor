using AliveMonitor.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AliveMonitor.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.GoogleId).IsRequired().HasMaxLength(128);
        builder.HasIndex(u => u.GoogleId).IsUnique();

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Name).IsRequired().HasMaxLength(256);
        builder.Property(u => u.AvatarUrl).HasMaxLength(1024);
        builder.Property(u => u.AlertEmail).IsRequired().HasMaxLength(256);

        builder.Property(u => u.TelegramChatId);
        builder.HasIndex(u => u.TelegramChatId).IsUnique();

        builder.Property(u => u.WebhookUrl).HasMaxLength(2048);

        builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("now()");
    }
}
