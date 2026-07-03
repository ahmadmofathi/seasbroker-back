using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.ChatId)
            .IsRequired();

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(m => m.IsAdmin)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.Created)
            .IsRequired();

        builder.Property(m => m.Updated)
            .IsRequired();

        builder.HasIndex(m => new { m.ChatId, m.Created })
            .HasDatabaseName("IX_messages_ChatId_Created");

        builder.HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
