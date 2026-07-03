using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Modules.Chat.Application.Services;

internal enum ChatTokenValidationResult
{
    Valid,
    Invalid,
    Expired,
}

internal static class ChatTokenValidator
{
    internal static ChatTokenValidationResult Validate(ChatToken? token, Guid expectedChatId, DateTime utcNow)
    {
        if (token is null || token.ChatId != expectedChatId)
        {
            return ChatTokenValidationResult.Invalid;
        }

        if (token.ExpiresAt <= utcNow)
        {
            return ChatTokenValidationResult.Expired;
        }

        return ChatTokenValidationResult.Valid;
    }
}
