using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Approval.Application.Exceptions;

namespace Seasbroker.Modules.Approval.Application.Helpers;

internal static class ApprovalDomainHelper
{
    public static Guid ParseGuidOrNotFound(string value, string resourceName = "resource")
    {
        if (!Guid.TryParse(value, out var parsedId))
        {
            throw new ApprovalException($"The requested {resourceName} wasn't found.", StatusCodes.Status404NotFound);
        }

        return parsedId;
    }

    public static async Task<Match> GetMatchOrThrowAsync(
        SeasbrokerDbContext dbContext,
        Guid matchId,
        CancellationToken cancellationToken)
    {
        var match = await dbContext.Matches
            .Include(m => m.VesselReservation)
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);

        if (match is null)
        {
            throw new ApprovalException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return match;
    }

    public static void EnsureNotLocked(Match match)
    {
        if (MatchStatus.LockedStatuses.Contains(match.Status))
        {
            throw new ApprovalException(
                "This match is completed and cannot be modified.",
                StatusCodes.Status400BadRequest);
        }
    }

    public static void ApplyExpectedRowVersion(Match match, byte[]? expectedRowVersion)
    {
        if (expectedRowVersion is null || expectedRowVersion.Length == 0)
        {
            return;
        }

        if (match.RowVersion.Length == 0 ||
            !match.RowVersion.SequenceEqual(expectedRowVersion))
        {
            throw new ApprovalException(
                "The match was modified by another user. Refresh and try again.",
                StatusCodes.Status409Conflict);
        }
    }

    public static byte[]? ParseRowVersion(string? rowVersion)
    {
        if (string.IsNullOrWhiteSpace(rowVersion))
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(rowVersion);
        }
        catch (FormatException)
        {
            throw new ApprovalException("Invalid rowVersion value.", StatusCodes.Status400BadRequest);
        }
    }

    public static async Task<VesselAvailability> FindCompatibleAvailabilityAsync(
        SeasbrokerDbContext dbContext,
        Guid vesselId,
        CargoListing cargo,
        CancellationToken cancellationToken)
    {
        var availability = await dbContext.VesselAvailabilities
            .FirstOrDefaultAsync(
                a => a.VesselId == vesselId &&
                     a.IsActive &&
                     a.AvailableFrom < cargo.ArrivalTime &&
                     a.AvailableTo > cargo.DepartureTime,
                cancellationToken);

        if (availability is null)
        {
            throw new ApprovalException(
                "No active vessel availability window is available for this match.",
                StatusCodes.Status400BadRequest);
        }

        return availability;
    }

    public static async Task EnsureNoApprovedMatchForCargoAsync(
        SeasbrokerDbContext dbContext,
        Guid cargoListingId,
        Guid currentMatchId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Matches
            .AsNoTracking()
            .AnyAsync(
                m => m.CargoListingId == cargoListingId &&
                     m.Id != currentMatchId &&
                     m.Status == MatchStatus.Approved,
                cancellationToken);

        if (exists)
        {
            throw new ApprovalException(
                "Another match is already approved for this cargo.",
                StatusCodes.Status409Conflict);
        }
    }
}
