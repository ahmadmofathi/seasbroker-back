using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.Constants;
using Seasbroker.Modules.Cargo.Application.Exceptions;

namespace Seasbroker.Modules.Cargo.Application.Helpers;

internal static class CargoDomainHelper
{
    public static Guid ParseCargoListingId(string cargoListingId)
    {
        if (!Guid.TryParse(cargoListingId, out var parsedId))
        {
            throw new CargoException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return parsedId;
    }

    public static Guid ParseCustomerId(string customerId)
    {
        if (!Guid.TryParse(customerId, out var parsedId))
        {
            throw new CargoException("Invalid customer identifier.", StatusCodes.Status400BadRequest);
        }

        return parsedId;
    }

    public static Guid? ParseOptionalQuoteId(string? requestedQuoteId)
    {
        if (string.IsNullOrWhiteSpace(requestedQuoteId))
        {
            return null;
        }

        if (!Guid.TryParse(requestedQuoteId, out var parsedId))
        {
            throw new CargoException("Invalid requested quote identifier.", StatusCodes.Status400BadRequest);
        }

        return parsedId;
    }

    public static async Task EnsureCustomerExistsAsync(
        SeasbrokerDbContext dbContext,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Id == customerId, cancellationToken);

        if (!exists)
        {
            throw new CargoException("The requested customer wasn't found.", StatusCodes.Status404NotFound);
        }
    }

    public static async Task EnsureQuoteExistsAsync(
        SeasbrokerDbContext dbContext,
        Guid quoteId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.RequestedQuotes
            .AsNoTracking()
            .AnyAsync(q => q.Id == quoteId, cancellationToken);

        if (!exists)
        {
            throw new CargoException("The requested quote wasn't found.", StatusCodes.Status404NotFound);
        }
    }

    public static async Task EnsureQuoteNotPromotedAsync(
        SeasbrokerDbContext dbContext,
        Guid quoteId,
        CancellationToken cancellationToken)
    {
        var alreadyPromoted = await dbContext.CargoListings
            .AsNoTracking()
            .AnyAsync(c => c.RequestedQuoteId == quoteId, cancellationToken);

        if (alreadyPromoted)
        {
            throw new CargoException(
                "This quote has already been promoted to a cargo listing.",
                StatusCodes.Status409Conflict);
        }
    }

    public static async Task<CargoListing> GetCargoListingOrThrowAsync(
        SeasbrokerDbContext dbContext,
        Guid cargoListingId,
        CancellationToken cancellationToken)
    {
        var listing = await dbContext.CargoListings
            .FirstOrDefaultAsync(c => c.Id == cargoListingId, cancellationToken);

        if (listing is null)
        {
            throw new CargoException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return listing;
    }

    public static void EnsureOpenForUpdate(CargoListing listing)
    {
        if (listing.Status != CargoStatus.Open)
        {
            throw new CargoException(
                "Cargo listing can only be updated while status is Open.",
                StatusCodes.Status400BadRequest);
        }
    }

    public static void ValidateDateRange(DateTime departureTime, DateTime arrivalTime)
    {
        if (departureTime >= arrivalTime)
        {
            throw new CargoException("DepartureTime must be before ArrivalTime.", StatusCodes.Status400BadRequest);
        }
    }

    public static string ResolveStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return CargoStatus.Open;
        }

        return status;
    }

    public static int ResolvePriority(int? priority)
    {
        return priority ?? CargoConstants.DefaultPriority;
    }

    public static async Task<string> GenerateReferenceNumberAsync(
        SeasbrokerDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var prefix = $"{CargoConstants.ReferenceNumberPrefix}-{DateTime.UtcNow:yyyyMMdd}-";

        var existingReferences = await dbContext.CargoListings
            .AsNoTracking()
            .Where(c => c.ReferenceNumber.StartsWith(prefix))
            .Select(c => c.ReferenceNumber)
            .ToListAsync(cancellationToken);

        var maxSequence = 0;

        foreach (var reference in existingReferences)
        {
            if (reference.Length <= prefix.Length)
            {
                continue;
            }

            if (int.TryParse(reference[prefix.Length..], out var sequence))
            {
                maxSequence = Math.Max(maxSequence, sequence);
            }
        }

        return $"{prefix}{(maxSequence + 1):D6}";
    }

    public static async Task EnsureReferenceNumberUniqueAsync(
        SeasbrokerDbContext dbContext,
        string referenceNumber,
        Guid? excludeCargoListingId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CargoListings
            .AsNoTracking()
            .Where(c => c.ReferenceNumber == referenceNumber);

        if (excludeCargoListingId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCargoListingId.Value);
        }

        if (await query.AnyAsync(cancellationToken))
        {
            throw new CargoException(
                "A cargo listing with this reference number already exists.",
                StatusCodes.Status409Conflict);
        }
    }
}
