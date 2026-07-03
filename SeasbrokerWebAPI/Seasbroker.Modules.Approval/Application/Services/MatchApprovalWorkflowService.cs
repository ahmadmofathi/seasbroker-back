using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Approval.Application.Abstractions;
using Seasbroker.Modules.Approval.Application.Commands;
using Seasbroker.Modules.Approval.Application.DTOs;
using Seasbroker.Modules.Approval.Application.Exceptions;
using Seasbroker.Modules.Approval.Application.Helpers;
using Seasbroker.Modules.Approval.Application.Queries;
using MatchingMatchRecordDto = Seasbroker.Modules.Matching.Application.DTOs.MatchRecordDto;
using Seasbroker.Modules.Matching.Application.Services;

namespace Seasbroker.Modules.Approval.Application.Services;

public interface IMatchApprovalWorkflowService
{
    Task<MatchApprovalRecordDto> ApproveAsync(
        string matchId,
        string? reason,
        string? rowVersion,
        CancellationToken cancellationToken = default);

    Task<MatchApprovalRecordDto> RejectAsync(
        string matchId,
        string? reason,
        string? rowVersion,
        CancellationToken cancellationToken = default);

    Task<MatchApprovalRecordDto> CancelAsync(
        string matchId,
        string? reason,
        string? rowVersion,
        CancellationToken cancellationToken = default);

    Task<MatchApprovalRecordDto> CompleteAsync(
        string matchId,
        string? reason,
        string? rowVersion,
        CancellationToken cancellationToken = default);

    Task<PocketBaseListResponse<MatchApprovalRecordDto>> GetPendingApprovalAsync(
        GetPendingApprovalMatchesQuery query,
        CancellationToken cancellationToken = default);

    Task<PocketBaseListResponse<MatchApprovalRecordDto>> GetApprovedAsync(
        GetApprovedMatchesQuery query,
        CancellationToken cancellationToken = default);
}

public class MatchApprovalWorkflowService : IMatchApprovalWorkflowService
{
    private readonly ICommandHandler<ApproveMatchCommand, MatchApprovalRecordDto> _approveHandler;
    private readonly ICommandHandler<RejectMatchCommand, MatchApprovalRecordDto> _rejectHandler;
    private readonly ICommandHandler<CancelMatchCommand, MatchApprovalRecordDto> _cancelHandler;
    private readonly ICommandHandler<CompleteMatchCommand, MatchApprovalRecordDto> _completeHandler;
    private readonly IQueryHandler<GetPendingApprovalMatchesQuery, PocketBaseListResponse<MatchApprovalRecordDto>> _pendingHandler;
    private readonly IQueryHandler<GetApprovedMatchesQuery, PocketBaseListResponse<MatchApprovalRecordDto>> _approvedHandler;
    private readonly IMatchService _matchingMatchService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly SeasbrokerDbContext _dbContext;

    public MatchApprovalWorkflowService(
        ICommandHandler<ApproveMatchCommand, MatchApprovalRecordDto> approveHandler,
        ICommandHandler<RejectMatchCommand, MatchApprovalRecordDto> rejectHandler,
        ICommandHandler<CancelMatchCommand, MatchApprovalRecordDto> cancelHandler,
        ICommandHandler<CompleteMatchCommand, MatchApprovalRecordDto> completeHandler,
        IQueryHandler<GetPendingApprovalMatchesQuery, PocketBaseListResponse<MatchApprovalRecordDto>> pendingHandler,
        IQueryHandler<GetApprovedMatchesQuery, PocketBaseListResponse<MatchApprovalRecordDto>> approvedHandler,
        IMatchService matchingMatchService,
        ICurrentUserAccessor currentUserAccessor,
        SeasbrokerDbContext dbContext)
    {
        _approveHandler = approveHandler;
        _rejectHandler = rejectHandler;
        _cancelHandler = cancelHandler;
        _completeHandler = completeHandler;
        _pendingHandler = pendingHandler;
        _approvedHandler = approvedHandler;
        _matchingMatchService = matchingMatchService;
        _currentUserAccessor = currentUserAccessor;
        _dbContext = dbContext;
    }

    public Task<MatchApprovalRecordDto> ApproveAsync(
        string matchId,
        string? reason,
        string? rowVersion,
        CancellationToken cancellationToken = default) =>
        _approveHandler.HandleAsync(
            new ApproveMatchCommand(
                matchId,
                _currentUserAccessor.GetRequiredUserId(),
                reason,
                ApprovalDomainHelper.ParseRowVersion(rowVersion)),
            cancellationToken);

    public Task<MatchApprovalRecordDto> RejectAsync(
        string matchId,
        string? reason,
        string? rowVersion,
        CancellationToken cancellationToken = default) =>
        _rejectHandler.HandleAsync(
            new RejectMatchCommand(
                matchId,
                _currentUserAccessor.GetRequiredUserId(),
                reason,
                ApprovalDomainHelper.ParseRowVersion(rowVersion)),
            cancellationToken);

    public async Task<MatchApprovalRecordDto> CancelAsync(
        string matchId,
        string? reason,
        string? rowVersion,
        CancellationToken cancellationToken = default)
    {
        var parsedMatchId = ApprovalDomainHelper.ParseGuidOrNotFound(matchId, "match");
        var status = await _dbContext.Matches
            .AsNoTracking()
            .Where(m => m.Id == parsedMatchId)
            .Select(m => m.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrEmpty(status))
        {
            throw new ApprovalException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        if (status == MatchStatus.Approved)
        {
            return await _cancelHandler.HandleAsync(
                new CancelMatchCommand(
                    matchId,
                    _currentUserAccessor.GetRequiredUserId(),
                    reason,
                    ApprovalDomainHelper.ParseRowVersion(rowVersion)),
                cancellationToken);
        }

        if (status is MatchStatus.Proposed or MatchStatus.PendingApproval)
        {
            var matchingResult = await _matchingMatchService.CancelAsync(matchId, cancellationToken);
            return MapFromMatchingDto(matchingResult);
        }

        throw new ApprovalException("This match cannot be cancelled.", StatusCodes.Status400BadRequest);
    }

    public Task<MatchApprovalRecordDto> CompleteAsync(
        string matchId,
        string? reason,
        string? rowVersion,
        CancellationToken cancellationToken = default) =>
        _completeHandler.HandleAsync(
            new CompleteMatchCommand(
                matchId,
                _currentUserAccessor.GetRequiredUserId(),
                reason,
                ApprovalDomainHelper.ParseRowVersion(rowVersion)),
            cancellationToken);

    public Task<PocketBaseListResponse<MatchApprovalRecordDto>> GetPendingApprovalAsync(
        GetPendingApprovalMatchesQuery query,
        CancellationToken cancellationToken = default) =>
        _pendingHandler.HandleAsync(query, cancellationToken);

    public Task<PocketBaseListResponse<MatchApprovalRecordDto>> GetApprovedAsync(
        GetApprovedMatchesQuery query,
        CancellationToken cancellationToken = default) =>
        _approvedHandler.HandleAsync(query, cancellationToken);

    private static MatchApprovalRecordDto MapFromMatchingDto(MatchingMatchRecordDto match) =>
        new()
        {
            Id = match.Id,
            CollectionId = match.CollectionId,
            CollectionName = match.CollectionName,
            Created = match.Created,
            Updated = match.Updated,
            CargoListingId = match.CargoListingId,
            VesselId = match.VesselId,
            Score = match.Score,
            Status = match.Status,
            Source = match.Source,
            MatchReason = match.MatchReason,
            ScoreBreakdown = match.ScoreBreakdown,
            ExpiresAt = match.ExpiresAt,
            ChatId = match.ChatId,
        };
}
