using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Application.Abstractions;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Events;
using Seasbroker.Modules.Matching.Application.Exceptions;
using Seasbroker.Modules.Matching.Application.Helpers;
using Seasbroker.Modules.Matching.Application.Mapping;

namespace Seasbroker.Modules.Matching.Application.Services;

public class MatchService : IMatchService
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public MatchService(SeasbrokerDbContext dbContext, IDomainEventDispatcher eventDispatcher)
    {
        _dbContext = dbContext;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<MatchRecordDto> CreateManualAsync(
        string cargoListingId,
        string vesselId,
        decimal? score,
        string? matchReason,
        CancellationToken cancellationToken = default)
    {
        var parsedCargoId = MatchingDomainHelper.ParseGuidOrNotFound(cargoListingId, "cargo listing");
        var parsedVesselId = MatchingDomainHelper.ParseGuidOrNotFound(vesselId, "vessel");

        await MatchingDomainHelper.GetOpenCargoOrThrowAsync(_dbContext, parsedCargoId, cancellationToken);
        await MatchingDomainHelper.GetActiveVesselOrThrowAsync(_dbContext, parsedVesselId, cancellationToken);
        await MatchingDomainHelper.EnsureNoActiveDuplicateAsync(_dbContext, parsedCargoId, parsedVesselId, cancellationToken);

        var vessel = await _dbContext.Vessels.AsNoTracking().FirstAsync(v => v.Id == parsedVesselId, cancellationToken);
        var cargo = await _dbContext.CargoListings.AsNoTracking().FirstAsync(c => c.Id == parsedCargoId, cancellationToken);

        if (vessel.Dwt < cargo.Weight)
        {
            throw new MatchingException(
                "Vessel capacity is insufficient for this cargo.",
                StatusCodes.Status400BadRequest);
        }

        var hasActiveAvailability = await _dbContext.VesselAvailabilities
            .AsNoTracking()
            .AnyAsync(
                a => a.VesselId == parsedVesselId &&
                     a.IsActive &&
                     a.AvailableFrom < cargo.ArrivalTime &&
                     a.AvailableTo > cargo.DepartureTime,
                cancellationToken);

        if (!hasActiveAvailability)
        {
            throw new MatchingException(
                "Vessel does not have an active availability window compatible with this cargo.",
                StatusCodes.Status400BadRequest);
        }

        var match = new Match
        {
            CargoListingId = parsedCargoId,
            VesselId = parsedVesselId,
            Score = score ?? 100m,
            Status = MatchStatus.PendingApproval,
            Source = MatchSource.Manual,
            MatchReason = string.IsNullOrWhiteSpace(matchReason)
                ? "Manually created match."
                : matchReason.Trim(),
        };

        _dbContext.Matches.Add(match);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new MatchingException(
                "An active match already exists for this cargo and vessel.",
                StatusCodes.Status409Conflict);
        }

        await _eventDispatcher.PublishAsync(
            new MatchPendingApprovalEvent(match.Id, match.CargoListingId, match.VesselId, match.Score, match.Source),
            cancellationToken);

        return MatchMapper.ToRecordDto(match);
    }

    public async Task<MatchRecordDto> ExpireAsync(string matchId, CancellationToken cancellationToken = default)
    {
        var parsedMatchId = MatchingDomainHelper.ParseGuidOrNotFound(matchId, "match");
        var match = await MatchingDomainHelper.GetMatchOrThrowAsync(_dbContext, parsedMatchId, cancellationToken);

        if (match.Status is not (MatchStatus.Proposed or MatchStatus.PendingApproval))
        {
            throw new MatchingException(
                "Only proposed or pending approval matches can be expired.",
                StatusCodes.Status400BadRequest);
        }

        match.Status = MatchStatus.Expired;
        match.ExpiresAt = null;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MatchMapper.ToRecordDto(match);
    }

    public async Task<MatchRecordDto> CancelAsync(string matchId, CancellationToken cancellationToken = default)
    {
        var parsedMatchId = MatchingDomainHelper.ParseGuidOrNotFound(matchId, "match");
        var match = await MatchingDomainHelper.GetMatchOrThrowAsync(_dbContext, parsedMatchId, cancellationToken);

        if (match.Status is MatchStatus.Approved or MatchStatus.Rejected or MatchStatus.Expired or MatchStatus.Cancelled)
        {
            throw new MatchingException(
                "This match cannot be cancelled.",
                StatusCodes.Status400BadRequest);
        }

        match.Status = MatchStatus.Cancelled;
        match.ExpiresAt = null;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MatchMapper.ToRecordDto(match);
    }
}
