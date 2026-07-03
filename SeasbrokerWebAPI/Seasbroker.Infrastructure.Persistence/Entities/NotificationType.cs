namespace Seasbroker.Infrastructure.Persistence.Entities;

public static class NotificationType
{
    public const string MatchPendingApproval = "MatchPendingApproval";

    public const string MatchApproved = "MatchApproved";

    public const string MatchRejected = "MatchRejected";

    public const string MatchCancelled = "MatchCancelled";

    public const string MatchCompleted = "MatchCompleted";

    public const string NewChatMessage = "NewChatMessage";

    public const string SystemNotification = "SystemNotification";
}
