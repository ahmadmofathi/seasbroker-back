namespace Seasbroker.Modules.Approval.Application.Events;

public sealed record MatchApprovedEvent(
    Guid MatchId,
    Guid CargoListingId,
    Guid VesselId,
    Guid ApprovedBy,
    decimal Score);

public sealed record MatchRejectedEvent(
    Guid MatchId,
    Guid CargoListingId,
    Guid VesselId,
    Guid RejectedBy,
    string? Reason);

public sealed record MatchCancelledEvent(
    Guid MatchId,
    Guid CargoListingId,
    Guid VesselId,
    Guid CancelledBy,
    string? Reason);

public sealed record MatchCompletedEvent(
    Guid MatchId,
    Guid CargoListingId,
    Guid VesselId,
    Guid CompletedBy);
