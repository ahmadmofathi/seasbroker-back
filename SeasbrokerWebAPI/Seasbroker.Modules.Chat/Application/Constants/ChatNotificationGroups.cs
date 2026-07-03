namespace Seasbroker.Modules.Chat.Application.Constants;

public static class ChatNotificationGroups
{
    public const string AdminChats = "admin:chats";

    public const string AdminMessages = "admin:messages";

    public static string ForChat(string chatId) => $"chat:{chatId}";
}
