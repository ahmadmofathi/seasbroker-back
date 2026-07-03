namespace Seasbroker.Modules.Chat.Application.Queries;

public sealed record GetMessagesByChatIdQuery(string ChatId, string Sort = "created");
