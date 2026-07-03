using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class VesselReservationConfiguration : IEntityTypeConfiguration<VesselReservation>
{
    public void Configure(EntityTypeBuilder<VesselReservation> builder)
    {
        builder.ToTable("vessel_reservations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.ReservedWeight)
            .IsRequired();

        builder.Property(r => r.IsReleased)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.Created)
            .IsRequired();

        builder.Property(r => r.Updated)
            .IsRequired();

        builder.HasIndex(r => r.MatchId)
            .IsUnique()
            .HasDatabaseName("IX_vessel_reservations_MatchId");

        builder.HasIndex(r => new { r.VesselId, r.IsReleased })
            .HasDatabaseName("IX_vessel_reservations_VesselId_IsReleased");

        builder.HasOne(r => r.Match)
            .WithOne(m => m.VesselReservation)
            .HasForeignKey<VesselReservation>(r => r.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Vessel)
            .WithMany()
            .HasForeignKey(r => r.VesselId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.VesselAvailability)
            .WithMany()
            .HasForeignKey(r => r.VesselAvailabilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CargoListing)
            .WithMany()
            .HasForeignKey(r => r.CargoListingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
