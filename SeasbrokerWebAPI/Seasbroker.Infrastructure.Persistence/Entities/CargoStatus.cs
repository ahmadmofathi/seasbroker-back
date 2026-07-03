namespace Seasbroker.Infrastructure.Persistence.Entities;

public static class CargoStatus
{
    public const string Draft = "Draft";

    public const string Open = "Open";

    public const string Matched = "Matched";

    public const string Closed = "Closed";

    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> MatchableStatuses = new HashSet<string> { Open };

    public static readonly IReadOnlySet<string> TerminalStatuses = new HashSet<string> { Closed, Cancelled };
}
