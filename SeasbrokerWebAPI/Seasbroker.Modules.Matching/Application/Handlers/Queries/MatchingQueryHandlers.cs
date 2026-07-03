using Seasbroker.Modules.Matching.Application.Abstractions;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Queries;
using Seasbroker.Modules.Matching.Application.Services;

namespace Seasbroker.Modules.Matching.Application.Handlers.Queries;

public class GetMatchesQueryHandler : IQueryHandler<GetMatchesQuery, PocketBaseListResponse<MatchRecordDto>>
{
    private readonly IMatchQueryService _matchQueryService;

    public GetMatchesQueryHandler(IMatchQueryService matchQueryService)
    {
        _matchQueryService = matchQueryService;
    }

    public Task<PocketBaseListResponse<MatchRecordDto>> HandleAsync(
        GetMatchesQuery query,
        CancellationToken cancellationToken = default) =>
        _matchQueryService.GetAllAsync(query, cancellationToken);
}

public class GetMatchByIdQueryHandler : IQueryHandler<GetMatchByIdQuery, MatchRecordDto>
{
    private readonly IMatchQueryService _matchQueryService;

    public GetMatchByIdQueryHandler(IMatchQueryService matchQueryService)
    {
        _matchQueryService = matchQueryService;
    }

    public Task<MatchRecordDto> HandleAsync(
        GetMatchByIdQuery query,
        CancellationToken cancellationToken = default) =>
        _matchQueryService.GetByIdAsync(query.MatchId, cancellationToken);
}

public class GetMatchingRulesQueryHandler : IQueryHandler<GetMatchingRulesQuery, IReadOnlyList<MatchingRuleRecordDto>>
{
    private readonly IMatchingRuleService _matchingRuleService;

    public GetMatchingRulesQueryHandler(IMatchingRuleService matchingRuleService)
    {
        _matchingRuleService = matchingRuleService;
    }

    public Task<IReadOnlyList<MatchingRuleRecordDto>> HandleAsync(
        GetMatchingRulesQuery query,
        CancellationToken cancellationToken = default) =>
        _matchingRuleService.GetAllAsync(cancellationToken);
}
