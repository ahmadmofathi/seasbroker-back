namespace Seasbroker.Modules.Approval.Application.Queries;

public sealed record GetPendingApprovalMatchesQuery(int Page = 1, int PerPage = 50);

public sealed record GetApprovedMatchesQuery(int Page = 1, int PerPage = 50);
