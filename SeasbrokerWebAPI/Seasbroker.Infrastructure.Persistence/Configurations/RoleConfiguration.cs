using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.Property(r => r.Created)
            .IsRequired();

        builder.Property(r => r.Updated)
            .IsRequired();

        builder.HasIndex(r => r.NormalizedName)
            .IsUnique()
            .HasDatabaseName("IX_roles_Name");
    }
}
