namespace Seasbroker.Modules.Approval.Application.Commands;

public sealed record ApproveMatchCommand(
    string MatchId,
    Guid ApprovedBy,
    string? Reason,
    byte[]? ExpectedRowVersion);

public sealed record RejectMatchCommand(
    string MatchId,
    Guid RejectedBy,
    string? Reason,
    byte[]? ExpectedRowVersion);

public sealed record CancelMatchCommand(
    string MatchId,
    Guid CancelledBy,
    string? Reason,
    byte[]? ExpectedRowVersion);

public sealed record CompleteMatchCommand(
    string MatchId,
    Guid CompletedBy,
    string? Reason,
    byte[]? ExpectedRowVersion);
