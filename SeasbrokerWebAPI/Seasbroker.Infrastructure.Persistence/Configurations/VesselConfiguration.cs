using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class VesselConfiguration : IEntityTypeConfiguration<Vessel>
{
    public void Configure(EntityTypeBuilder<Vessel> builder)
    {
        builder.ToTable("vessels", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_vessels_Dwt", "[Dwt] > 0");
        });

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.ImoNumber)
            .HasMaxLength(20);

        builder.Property(v => v.VesselType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Dwt)
            .IsRequired();

        builder.Property(v => v.CurrentPort)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.FlagCountry)
            .HasMaxLength(100);

        builder.Property(v => v.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Notes)
            .HasMaxLength(2000);

        builder.Property(v => v.Created)
            .IsRequired();

        builder.Property(v => v.Updated)
            .IsRequired();

        builder.HasIndex(v => v.Status)
            .HasDatabaseName("IX_vessels_Status");

        builder.HasIndex(v => v.VesselType)
            .HasDatabaseName("IX_vessels_VesselType");

        builder.HasIndex(v => v.CurrentPort)
            .HasDatabaseName("IX_vessels_CurrentPort");

        builder.HasIndex(v => v.CustomerId)
            .HasDatabaseName("IX_vessels_CustomerId");

        builder.HasIndex(v => v.ImoNumber)
            .IsUnique()
            .HasFilter("[ImoNumber] IS NOT NULL")
            .HasDatabaseName("IX_vessels_ImoNumber");

        builder.HasOne(v => v.Customer)
            .WithMany()
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
