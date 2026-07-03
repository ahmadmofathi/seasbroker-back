using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .ValueGeneratedNever();

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(n => n.NotificationType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(n => n.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.Payload)
            .HasMaxLength(8000);

        builder.HasIndex(n => new { n.UserId, n.Status, n.CreatedAt })
            .HasDatabaseName("IX_notifications_UserId_Status_CreatedAt");

        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
            .HasDatabaseName("IX_notifications_UserId_CreatedAt");
    }
}
