using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Approval.Application.Abstractions;
using Seasbroker.Modules.Approval.Application.Commands;
using Seasbroker.Modules.Approval.Application.DTOs;
using Seasbroker.Modules.Approval.Application.Events;
using Seasbroker.Modules.Approval.Application.Exceptions;
using Seasbroker.Modules.Approval.Application.Helpers;
using Seasbroker.Modules.Approval.Application.Mapping;
using Seasbroker.Modules.Approval.Application.Queries;

namespace Seasbroker.Modules.Approval.Application.Services;

public interface IMatchApprovalService
{
    Task<MatchApprovalRecordDto> ApproveAsync(
        ApproveMatchCommand command,
        CancellationToken cancellationToken = default);

    Task<MatchApprovalRecordDto> RejectAsync(
        RejectMatchCommand command,
        CancellationToken cancellationToken = default);

    Task<MatchApprovalRecordDto> CancelApprovedAsync(
        CancelMatchCommand command,
        CancellationToken cancellationToken = default);

    Task<MatchApprovalRecordDto> CompleteAsync(
        CompleteMatchCommand command,
        CancellationToken cancellationToken = default);
}

public interface IMatchApprovalQueryService
{
    Task<PocketBaseListResponse<MatchApprovalRecordDto>> GetPendingApprovalAsync(
        GetPendingApprovalMatchesQuery query,
        CancellationToken cancellationToken = default);

    Task<PocketBaseListResponse<MatchApprovalRecordDto>> GetApprovedAsync(
        GetApprovedMatchesQuery query,
        CancellationToken cancellationToken = default);
}

public class MatchApprovalService : IMatchApprovalService
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public MatchApprovalService(
        SeasbrokerDbContext dbContext,
        IDomainEventDispatcher eventDispatcher)
    {
        _dbContext = dbContext;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<MatchApprovalRecordDto> ApproveAsync(
        ApproveMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var matchId = ApprovalDomainHelper.ParseGuidOrNotFound(command.MatchId, "match");
        var match = await ApprovalDomainHelper.GetMatchOrThrowAsync(_dbContext, matchId, cancellationToken);

        ApprovalDomainHelper.EnsureNotLocked(match);
        ApprovalDomainHelper.ApplyExpectedRowVersion(match, command.ExpectedRowVersion);

        if (match.Status != MatchStatus.PendingApproval)
        {
            throw new ApprovalException(
                "Only pending approval matches can be approved.",
                StatusCodes.Status400BadRequest);
        }

        await ApprovalDomainHelper.EnsureNoApprovedMatchForCargoAsync(
            _dbContext,
            match.CargoListingId,
            match.Id,
            cancellationToken);

        var cargo = await _dbContext.CargoListings
            .FirstAsync(c => c.Id == match.CargoListingId, cancellationToken);

        if (cargo.Status != CargoStatus.Open && cargo.Status != CargoStatus.Matched)
        {
            throw new ApprovalException(
                "Cargo must be open to approve a match.",
                StatusCodes.Status400BadRequest);
        }

        var utcNow = DateTime.UtcNow;
        var availability = await ApprovalDomainHelper.FindCompatibleAvailabilityAsync(
            _dbContext,
            match.VesselId,
            cargo,
            cancellationToken);

        match.Status = MatchStatus.Approved;
        match.ApprovedBy = command.ApprovedBy;
        match.ApprovedAt = utcNow;
        match.Reason = command.Reason?.Trim();
        match.ExpiresAt = null;

        cargo.Status = CargoStatus.Matched;

        var reservation = new VesselReservation
        {
            MatchId = match.Id,
            VesselId = match.VesselId,
            VesselAvailabilityId = availability.Id,
            CargoListingId = match.CargoListingId,
            ReservedWeight = cargo.Weight,
            IsReleased = false,
        };

        availability.IsActive = false;
        _dbContext.VesselReservations.Add(reservation);

        await RejectCompetingPendingMatchesAsync(
            match.CargoListingId,
            match.Id,
            command.ApprovedBy,
            utcNow,
            cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ApprovalException(
                "The match was modified by another user. Refresh and try again.",
                StatusCodes.Status409Conflict);
        }
        catch (DbUpdateException)
        {
            throw new ApprovalException(
                "Another match is already approved for this cargo.",
                StatusCodes.Status409Conflict);
        }

        await _eventDispatcher.PublishAsync(
            new MatchApprovedEvent(match.Id, match.CargoListingId, match.VesselId, command.ApprovedBy, match.Score),
            cancellationToken);

        return ApprovalMapper.ToRecordDto(match);
    }

    public async Task<MatchApprovalRecordDto> RejectAsync(
        RejectMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var matchId = ApprovalDomainHelper.ParseGuidOrNotFound(command.MatchId, "match");
        var match = await ApprovalDomainHelper.GetMatchOrThrowAsync(_dbContext, matchId, cancellationToken);

        ApprovalDomainHelper.EnsureNotLocked(match);
        ApprovalDomainHelper.ApplyExpectedRowVersion(match, command.ExpectedRowVersion);

        if (match.Status != MatchStatus.PendingApproval)
        {
            throw new ApprovalException(
                "Only pending approval matches can be rejected.",
                StatusCodes.Status400BadRequest);
        }

        var utcNow = DateTime.UtcNow;
        match.Status = MatchStatus.Rejected;
        match.RejectedBy = command.RejectedBy;
        match.RejectedAt = utcNow;
        match.Reason = command.Reason?.Trim();
        match.ExpiresAt = null;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ApprovalException(
                "The match was modified by another user. Refresh and try again.",
                StatusCodes.Status409Conflict);
        }

        await _eventDispatcher.PublishAsync(
            new MatchRejectedEvent(match.Id, match.CargoListingId, match.VesselId, command.RejectedBy, match.Reason),
            cancellationToken);

        return ApprovalMapper.ToRecordDto(match);
    }

    public async Task<MatchApprovalRecordDto> CancelApprovedAsync(
        CancelMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var matchId = ApprovalDomainHelper.ParseGuidOrNotFound(command.MatchId, "match");
        var match = await ApprovalDomainHelper.GetMatchOrThrowAsync(_dbContext, matchId, cancellationToken);

        ApprovalDomainHelper.EnsureNotLocked(match);
        ApprovalDomainHelper.ApplyExpectedRowVersion(match, command.ExpectedRowVersion);

        if (match.Status != MatchStatus.Approved)
        {
            throw new ApprovalException(
                "Only approved matches can be cancelled through the approval workflow.",
                StatusCodes.Status400BadRequest);
        }

        var utcNow = DateTime.UtcNow;
        match.Status = MatchStatus.Cancelled;
        match.CancelledBy = command.CancelledBy;
        match.CancelledAt = utcNow;
        match.Reason = command.Reason?.Trim();

        await ReleaseVesselReservationAsync(match, utcNow, cancellationToken);
        await ReopenCargoIfNeededAsync(match.CargoListingId, match.Id, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ApprovalException(
                "The match was modified by another user. Refresh and try again.",
                StatusCodes.Status409Conflict);
        }

        await _eventDispatcher.PublishAsync(
            new MatchCancelledEvent(match.Id, match.CargoListingId, match.VesselId, command.CancelledBy, match.Reason),
            cancellationToken);

        return ApprovalMapper.ToRecordDto(match);
    }

    public async Task<MatchApprovalRecordDto> CompleteAsync(
        CompleteMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var matchId = ApprovalDomainHelper.ParseGuidOrNotFound(command.MatchId, "match");
        var match = await ApprovalDomainHelper.GetMatchOrThrowAsync(_dbContext, matchId, cancellationToken);

        ApprovalDomainHelper.EnsureNotLocked(match);
        ApprovalDomainHelper.ApplyExpectedRowVersion(match, command.ExpectedRowVersion);

        if (match.Status != MatchStatus.Approved)
        {
            throw new ApprovalException(
                "Only approved matches can be completed.",
                StatusCodes.Status400BadRequest);
        }

        var utcNow = DateTime.UtcNow;
        match.Status = MatchStatus.Completed;
        match.CompletedBy = command.CompletedBy;
        match.CompletedAt = utcNow;
        match.Reason = command.Reason?.Trim();

        var cargo = await _dbContext.CargoListings
            .FirstAsync(c => c.Id == match.CargoListingId, cancellationToken);

        if (cargo.Status == CargoStatus.Matched)
        {
            cargo.Status = CargoStatus.Closed;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ApprovalException(
                "The match was modified by another user. Refresh and try again.",
                StatusCodes.Status409Conflict);
        }

        await _eventDispatcher.PublishAsync(
            new MatchCompletedEvent(match.Id, match.CargoListingId, match.VesselId, command.CompletedBy),
            cancellationToken);

        return ApprovalMapper.ToRecordDto(match);
    }

    private async Task RejectCompetingPendingMatchesAsync(
        Guid cargoListingId,
        Guid approvedMatchId,
        Guid rejectedBy,
        DateTime rejectedAt,
        CancellationToken cancellationToken)
    {
        var competing = await _dbContext.Matches
            .Where(m => m.CargoListingId == cargoListingId &&
                        m.Id != approvedMatchId &&
                        m.Status == MatchStatus.PendingApproval)
            .ToListAsync(cancellationToken);

        foreach (var pending in competing)
        {
            pending.Status = MatchStatus.Rejected;
            pending.RejectedBy = rejectedBy;
            pending.RejectedAt = rejectedAt;
            pending.Reason = $"Automatically rejected because match {approvedMatchId} was approved.";
            pending.ExpiresAt = null;
        }
    }

    private async Task ReleaseVesselReservationAsync(
        Match match,
        DateTime releasedAt,
        CancellationToken cancellationToken)
    {
        var reservation = match.VesselReservation ??
            await _dbContext.VesselReservations
                .FirstOrDefaultAsync(r => r.MatchId == match.Id && !r.IsReleased, cancellationToken);

        if (reservation is null)
        {
            return;
        }

        reservation.IsReleased = true;
        reservation.ReleasedAt = releasedAt;

        var availability = await _dbContext.VesselAvailabilities
            .FirstOrDefaultAsync(a => a.Id == reservation.VesselAvailabilityId, cancellationToken);

        if (availability is not null)
        {
            availability.IsActive = true;
        }
    }

    private async Task ReopenCargoIfNeededAsync(
        Guid cargoListingId,
        Guid cancelledMatchId,
        CancellationToken cancellationToken)
    {
        var cargo = await _dbContext.CargoListings
            .FirstAsync(c => c.Id == cargoListingId, cancellationToken);

        if (cargo.Status != CargoStatus.Matched)
        {
            return;
        }

        var hasOtherApproved = await _dbContext.Matches
            .AsNoTracking()
            .AnyAsync(
                m => m.CargoListingId == cargoListingId &&
                     m.Id != cancelledMatchId &&
                     m.Status == MatchStatus.Approved,
                cancellationToken);

        if (!hasOtherApproved)
        {
            cargo.Status = CargoStatus.Open;
        }
    }
}

public class MatchApprovalQueryService : IMatchApprovalQueryService
{
    private readonly SeasbrokerDbContext _dbContext;

    public MatchApprovalQueryService(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PocketBaseListResponse<MatchApprovalRecordDto>> GetPendingApprovalAsync(
        GetPendingApprovalMatchesQuery query,
        CancellationToken cancellationToken = default) =>
        GetByStatusAsync(MatchStatus.PendingApproval, query.Page, query.PerPage, cancellationToken);

    public Task<PocketBaseListResponse<MatchApprovalRecordDto>> GetApprovedAsync(
        GetApprovedMatchesQuery query,
        CancellationToken cancellationToken = default) =>
        GetByStatusAsync(MatchStatus.Approved, query.Page, query.PerPage, cancellationToken);

    private async Task<PocketBaseListResponse<MatchApprovalRecordDto>> GetByStatusAsync(
        string status,
        int page,
        int perPage,
        CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        perPage = perPage < 1 ? 50 : Math.Min(perPage, 200);

        var matchesQuery = _dbContext.Matches
            .AsNoTracking()
            .Where(m => m.Status == status);

        var totalItems = await matchesQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)perPage);

        var matches = await matchesQuery
            .OrderByDescending(m => m.Score)
            .ThenByDescending(m => m.Created)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        return new PocketBaseListResponse<MatchApprovalRecordDto>
        {
            Page = page,
            PerPage = perPage,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = matches.Select(ApprovalMapper.ToRecordDto).ToList(),
        };
    }
}
