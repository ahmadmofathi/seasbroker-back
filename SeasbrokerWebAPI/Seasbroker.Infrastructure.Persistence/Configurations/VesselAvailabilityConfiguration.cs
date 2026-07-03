using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class VesselAvailabilityConfiguration : IEntityTypeConfiguration<VesselAvailability>
{
    public void Configure(EntityTypeBuilder<VesselAvailability> builder)
    {
        builder.ToTable("vessel_availabilities", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_vessel_availabilities_DateRange",
                "[AvailableFrom] < [AvailableTo]");
        });

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.VesselId)
            .IsRequired();

        builder.Property(a => a.AvailableFrom)
            .IsRequired();

        builder.Property(a => a.AvailableTo)
            .IsRequired();

        builder.Property(a => a.OpenPort)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.DestinationPort)
            .HasMaxLength(200);

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.Created)
            .IsRequired();

        builder.Property(a => a.Updated)
            .IsRequired();

        builder.HasIndex(a => new { a.VesselId, a.IsActive })
            .HasDatabaseName("IX_vessel_availabilities_VesselId_IsActive");

        builder.HasIndex(a => new { a.OpenPort, a.AvailableFrom, a.AvailableTo })
            .HasDatabaseName("IX_vessel_availabilities_OpenPort_Dates");

        builder.HasOne(a => a.Vessel)
            .WithMany(v => v.Availabilities)
            .HasForeignKey(a => a.VesselId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
