using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Application.Exceptions;

namespace Seasbroker.Modules.Matching.Application.Helpers;

internal static class MatchingDomainHelper
{
    public static Guid ParseGuidOrNotFound(string value, string resourceName = "resource")
    {
        if (!Guid.TryParse(value, out var parsedId))
        {
            throw new MatchingException($"The requested {resourceName} wasn't found.", StatusCodes.Status404NotFound);
        }

        return parsedId;
    }

    public static async Task<CargoListing> GetOpenCargoOrThrowAsync(
        SeasbrokerDbContext dbContext,
        Guid cargoListingId,
        CancellationToken cancellationToken)
    {
        var cargo = await dbContext.CargoListings
            .FirstOrDefaultAsync(c => c.Id == cargoListingId, cancellationToken);

        if (cargo is null)
        {
            throw new MatchingException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        if (cargo.Status != CargoStatus.Open)
        {
            throw new MatchingException(
                "Matching can only run for cargo listings with Open status.",
                StatusCodes.Status400BadRequest);
        }

        return cargo;
    }

    public static async Task<Vessel> GetActiveVesselOrThrowAsync(
        SeasbrokerDbContext dbContext,
        Guid vesselId,
        CancellationToken cancellationToken)
    {
        var vessel = await dbContext.Vessels
            .FirstOrDefaultAsync(v => v.Id == vesselId, cancellationToken);

        if (vessel is null)
        {
            throw new MatchingException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        if (vessel.Status != VesselStatus.Active)
        {
            throw new MatchingException(
                "Matching can only run for active vessels.",
                StatusCodes.Status400BadRequest);
        }

        return vessel;
    }

    public static async Task<Match> GetMatchOrThrowAsync(
        SeasbrokerDbContext dbContext,
        Guid matchId,
        CancellationToken cancellationToken)
    {
        var match = await dbContext.Matches
            .FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);

        if (match is null)
        {
            throw new MatchingException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return match;
    }

    public static async Task EnsureNoActiveDuplicateAsync(
        SeasbrokerDbContext dbContext,
        Guid cargoListingId,
        Guid vesselId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Matches
            .AsNoTracking()
            .AnyAsync(
                m => m.CargoListingId == cargoListingId &&
                     m.VesselId == vesselId &&
                     MatchStatus.ActivePairStatusFilter.Contains(m.Status),
                cancellationToken);

        if (exists)
        {
            throw new MatchingException(
                "An active match already exists for this cargo and vessel.",
                StatusCodes.Status409Conflict);
        }
    }

    public static async Task<MatchingRule> GetMatchingRuleOrThrowAsync(
        SeasbrokerDbContext dbContext,
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var rule = await dbContext.MatchingRules
            .FirstOrDefaultAsync(r => r.Id == ruleId, cancellationToken);

        if (rule is null)
        {
            throw new MatchingException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return rule;
    }
}
