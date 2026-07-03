using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class MatchingRuleConfiguration : IEntityTypeConfiguration<MatchingRule>
{
    public void Configure(EntityTypeBuilder<MatchingRule> builder)
    {
        builder.ToTable("matching_rules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Criterion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Weight)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.Configuration)
            .HasMaxLength(2000);

        builder.Property(r => r.Created)
            .IsRequired();

        builder.Property(r => r.Updated)
            .IsRequired();

        builder.HasIndex(r => r.Criterion)
            .IsUnique()
            .HasDatabaseName("IX_matching_rules_Criterion");
    }
}
