namespace Seasbroker.Modules.Chat.Application.Constants;

public static class ChatConstants
{
    public const string ChatTokenCookieName = "chatToken";

    public const int ChatTokenCookieMaxAgeSeconds = 86400;

    public const int ChatTokenExpiryHours = 24;

    public const string SuperuserRole = "Superuser";

    public const string SuperuserPolicy = "Superuser";

    public const string ChatsCollectionName = "chats";

    public const string MessagesCollectionName = "messages";
}
