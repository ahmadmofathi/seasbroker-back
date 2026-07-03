namespace Seasbroker.Modules.Matching.Application.Commands;

public sealed record RunMatchingCommand(string? CargoListingId, string? VesselId);

public sealed record RunMatchingForCargoCommand(string CargoListingId);

public sealed record RunMatchingForVesselCommand(string VesselId);

public sealed record RunMatchingBatchCommand;

public sealed record CreateManualMatchCommand(
    string CargoListingId,
    string VesselId,
    decimal? Score,
    string? MatchReason);

public sealed record ExpireMatchCommand(string MatchId);

public sealed record CancelMatchCommand(string MatchId);

public sealed record UpdateMatchingRuleCommand(
    string RuleId,
    decimal? Weight,
    bool? IsActive,
    string? Configuration);
