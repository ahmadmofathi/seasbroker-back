namespace Seasbroker.Modules.Matching.Application.Events;

public sealed record MatchPendingApprovalEvent(
    Guid MatchId,
    Guid CargoListingId,
    Guid VesselId,
    decimal Score,
    string Source);
