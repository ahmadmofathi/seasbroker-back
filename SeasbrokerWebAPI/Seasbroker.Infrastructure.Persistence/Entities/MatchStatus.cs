namespace Seasbroker.Infrastructure.Persistence.Entities;

public static class MatchStatus
{
    public const string Proposed = "Proposed";

    public const string PendingApproval = "PendingApproval";

    public const string Approved = "Approved";

    public const string Rejected = "Rejected";

    public const string Expired = "Expired";

    public const string Cancelled = "Cancelled";

    public const string Completed = "Completed";

    public static readonly IReadOnlySet<string> ActivePairStatuses = new HashSet<string>
    {
        Proposed,
        PendingApproval,
        Approved,
    };

    /// <summary>
    /// EF-translatable status list for active cargo/vessel pair checks.
    /// </summary>
    public static readonly string[] ActivePairStatusFilter =
    [
        Proposed,
        PendingApproval,
        Approved,
    ];

    public static readonly IReadOnlySet<string> TerminalStatuses = new HashSet<string>
    {
        Rejected,
        Cancelled,
        Expired,
        Completed,
    };

    /// <summary>
    /// EF-translatable status list for terminal match checks.
    /// </summary>
    public static readonly string[] TerminalStatusFilter =
    [
        Rejected,
        Cancelled,
        Expired,
        Completed,
    ];

    public static readonly IReadOnlySet<string> LockedStatuses = new HashSet<string>
    {
        Completed,
    };
}
