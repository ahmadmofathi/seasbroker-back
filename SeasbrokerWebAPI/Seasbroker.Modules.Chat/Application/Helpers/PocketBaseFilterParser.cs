namespace Seasbroker.Modules.Chat.Application.Helpers;

public static class PocketBaseFilterParser
{
    public static string? TryParseChatIdEquals(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        const string prefix = "chatId = ";
        var trimmed = filter.Trim();

        if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        return trimmed[prefix.Length..].Trim().Trim('"');
    }
}
