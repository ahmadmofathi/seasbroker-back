using Seasbroker.Modules.Approval.Application.Abstractions;
using Seasbroker.Modules.Approval.Application.DTOs;
using Seasbroker.Modules.Approval.Application.Queries;
using Seasbroker.Modules.Approval.Application.Services;

namespace Seasbroker.Modules.Approval.Application.Handlers.Queries;

public class GetPendingApprovalMatchesQueryHandler
    : IQueryHandler<GetPendingApprovalMatchesQuery, PocketBaseListResponse<MatchApprovalRecordDto>>
{
    private readonly IMatchApprovalQueryService _queryService;

    public GetPendingApprovalMatchesQueryHandler(IMatchApprovalQueryService queryService)
    {
        _queryService = queryService;
    }

    public Task<PocketBaseListResponse<MatchApprovalRecordDto>> HandleAsync(
        GetPendingApprovalMatchesQuery query,
        CancellationToken cancellationToken = default) =>
        _queryService.GetPendingApprovalAsync(query, cancellationToken);
}

public class GetApprovedMatchesQueryHandler
    : IQueryHandler<GetApprovedMatchesQuery, PocketBaseListResponse<MatchApprovalRecordDto>>
{
    private readonly IMatchApprovalQueryService _queryService;

    public GetApprovedMatchesQueryHandler(IMatchApprovalQueryService queryService)
    {
        _queryService = queryService;
    }

    public Task<PocketBaseListResponse<MatchApprovalRecordDto>> HandleAsync(
        GetApprovedMatchesQuery query,
        CancellationToken cancellationToken = default) =>
        _queryService.GetApprovedAsync(query, cancellationToken);
}
