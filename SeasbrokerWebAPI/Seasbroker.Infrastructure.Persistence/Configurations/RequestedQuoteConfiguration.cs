using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Infrastructure.Persistence.Configurations;

public class RequestedQuoteConfiguration : IEntityTypeConfiguration<RequestedQuote>
{
    public void Configure(EntityTypeBuilder<RequestedQuote> builder)
    {
        builder.ToTable("requested_quotes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .ValueGeneratedNever();

        builder.Property(q => q.CustomerId)
            .IsRequired();

        builder.Property(q => q.CargoType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(q => q.Weight)
            .IsRequired();

        builder.Property(q => q.DeparturePort)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(q => q.DepartureTime)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(q => q.ArrivalPort)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(q => q.ArrivalTime)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(q => q.Dimensions)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(q => q.AdditionalInfo)
            .HasMaxLength(2000);

        builder.Property(q => q.Created)
            .IsRequired();

        builder.Property(q => q.Updated)
            .IsRequired();

        builder.HasIndex(q => q.CustomerId)
            .HasDatabaseName("IX_requested_quotes_CustomerId");

        builder.HasOne(q => q.Customer)
            .WithMany(c => c.RequestedQuotes)
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
