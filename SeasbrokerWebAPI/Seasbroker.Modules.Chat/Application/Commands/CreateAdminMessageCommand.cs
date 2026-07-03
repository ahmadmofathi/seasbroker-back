namespace Seasbroker.Modules.Chat.Application.Commands;

public sealed record CreateAdminMessageCommand(string ChatId, string Content);
