using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_matches_Score", "[Score] >= 0 AND [Score] <= 100");
        });

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.CargoListingId)
            .IsRequired();

        builder.Property(m => m.VesselId)
            .IsRequired();

        builder.Property(m => m.Score)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(m => m.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Source)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.MatchReason)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.ScoreBreakdown)
            .HasMaxLength(4000);

        builder.Property(m => m.Created)
            .IsRequired();

        builder.Property(m => m.Updated)
            .IsRequired();

        builder.Property(m => m.Reason)
            .HasMaxLength(2000);

        builder.Property(m => m.RowVersion)
            .IsRowVersion()
            .IsRequired();

        builder.HasIndex(m => new { m.CargoListingId, m.Status })
            .HasDatabaseName("IX_matches_CargoListingId_Status");

        builder.HasIndex(m => new { m.VesselId, m.Status })
            .HasDatabaseName("IX_matches_VesselId_Status");

        builder.HasIndex(m => new { m.Status, m.ExpiresAt })
            .HasDatabaseName("IX_matches_Status_ExpiresAt");

        builder.HasIndex(m => new { m.CargoListingId, m.VesselId })
            .IsUnique()
            .HasFilter("[Status] IN ('Proposed', 'PendingApproval', 'Approved')")
            .HasDatabaseName("IX_matches_CargoListingId_VesselId_Active");

        builder.HasIndex(m => m.CargoListingId)
            .IsUnique()
            .HasFilter("[Status] = 'Approved'")
            .HasDatabaseName("IX_matches_CargoListingId_Approved");

        builder.HasOne(m => m.CargoListing)
            .WithMany()
            .HasForeignKey(m => m.CargoListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Vessel)
            .WithMany()
            .HasForeignKey(m => m.VesselId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Chat)
            .WithMany()
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
