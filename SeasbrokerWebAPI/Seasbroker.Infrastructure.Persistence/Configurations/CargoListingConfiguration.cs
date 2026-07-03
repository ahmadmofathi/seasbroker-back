using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class CargoListingConfiguration : IEntityTypeConfiguration<CargoListing>
{
    public void Configure(EntityTypeBuilder<CargoListing> builder)
    {
        builder.ToTable("cargo_listings", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_cargo_listings_Weight", "[Weight] > 0");
            tableBuilder.HasCheckConstraint("CK_cargo_listings_Priority", "[Priority] >= 1 AND [Priority] <= 5");
            tableBuilder.HasCheckConstraint(
                "CK_cargo_listings_DateRange",
                "[DepartureTime] < [ArrivalTime]");
        });

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.CustomerId)
            .IsRequired();

        builder.Property(c => c.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.CargoType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Weight)
            .IsRequired();

        builder.Property(c => c.Dimensions)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.DeparturePort)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.DepartureTime)
            .IsRequired();

        builder.Property(c => c.ArrivalPort)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ArrivalTime)
            .IsRequired();

        builder.Property(c => c.AdditionalInfo)
            .HasMaxLength(2000);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Priority)
            .IsRequired();

        builder.Property(c => c.Created)
            .IsRequired();

        builder.Property(c => c.Updated)
            .IsRequired();

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_cargo_listings_Status");

        builder.HasIndex(c => new { c.DeparturePort, c.ArrivalPort })
            .HasDatabaseName("IX_cargo_listings_Ports");

        builder.HasIndex(c => new { c.DepartureTime, c.ArrivalTime })
            .HasDatabaseName("IX_cargo_listings_Dates");

        builder.HasIndex(c => c.CustomerId)
            .HasDatabaseName("IX_cargo_listings_CustomerId");

        builder.HasIndex(c => c.ReferenceNumber)
            .IsUnique()
            .HasDatabaseName("IX_cargo_listings_ReferenceNumber");

        builder.HasIndex(c => c.RequestedQuoteId)
            .IsUnique()
            .HasFilter("[RequestedQuoteId] IS NOT NULL")
            .HasDatabaseName("IX_cargo_listings_RequestedQuoteId");

        builder.HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.RequestedQuote)
            .WithMany()
            .HasForeignKey(c => c.RequestedQuoteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
