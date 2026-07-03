namespace Seasbroker.Modules.Matching.Application.Queries;

public sealed record GetMatchesQuery(
    string? Status = null,
    string? CargoListingId = null,
    string? VesselId = null,
    int Page = 1,
    int PerPage = 50);

public sealed record GetMatchByIdQuery(string MatchId);

public sealed record GetMatchesForCargoQuery(string CargoListingId);

public sealed record GetMatchesForVesselQuery(string VesselId);

public sealed record GetMatchingRulesQuery;

public sealed record GetMatchingRuleByIdQuery(string RuleId);
