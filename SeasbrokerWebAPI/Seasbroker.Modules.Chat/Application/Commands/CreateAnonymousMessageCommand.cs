namespace Seasbroker.Modules.Chat.Application.Commands;

public sealed record CreateAnonymousMessageCommand(string Token, string ChatId, string Content);
