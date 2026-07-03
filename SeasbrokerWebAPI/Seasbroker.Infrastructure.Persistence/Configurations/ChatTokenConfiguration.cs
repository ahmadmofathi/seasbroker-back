using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class ChatTokenConfiguration : IEntityTypeConfiguration<ChatToken>
{
    public void Configure(EntityTypeBuilder<ChatToken> builder)
    {
        builder.ToTable("chat_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(t => t.ChatId)
            .IsRequired();

        builder.Property(t => t.ExpiresAt)
            .IsRequired();

        builder.Property(t => t.Created)
            .IsRequired();

        builder.Property(t => t.Updated)
            .IsRequired();

        builder.HasIndex(t => t.Token)
            .IsUnique()
            .HasDatabaseName("IX_chat_tokens_Token");

        builder.HasIndex(t => t.ChatId)
            .HasDatabaseName("IX_chat_tokens_ChatId");

        builder.HasOne(t => t.Chat)
            .WithMany(c => c.ChatTokens)
            .HasForeignKey(t => t.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
