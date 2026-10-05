namespace Seasbroker.Modules.Chat.Application.Constants;

public static class ChatConstants
{
    public const string ChatTokenCookieName = "chatToken";

    /// <summary>A visitor's chat stays reachable for a week, so they can come back and read our reply.</summary>
    public const int ChatTokenExpiryHours = 24 * 7;

    public const int ChatTokenCookieMaxAgeSeconds = ChatTokenExpiryHours * 3600;

    public const string SuperuserRole = "Superuser";

    public const string SuperuserPolicy = "Superuser";

    public const string ChatsCollectionName = "chats";

    public const string MessagesCollectionName = "messages";
}
