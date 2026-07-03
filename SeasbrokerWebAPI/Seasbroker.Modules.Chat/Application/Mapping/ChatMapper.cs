using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Chat.Application.Constants;
using Seasbroker.Modules.Chat.Application.DTOs;

namespace Seasbroker.Modules.Chat.Application.Mapping;

public static class ChatMapper
{
    public static ChatRecordDto ToRecordDto(global::Seasbroker.Infrastructure.Persistence.Entities.Chat chat)
    {
        return new ChatRecordDto
        {
            Id = chat.Id.ToString(),
            CollectionId = ChatConstants.ChatsCollectionName,
            CollectionName = ChatConstants.ChatsCollectionName,
            Created = chat.Created,
            Updated = chat.Updated,
            Name = chat.Name,
        };
    }

    public static MessageRecordDto ToRecordDto(global::Seasbroker.Infrastructure.Persistence.Entities.Message message)
    {
        return new MessageRecordDto
        {
            Id = message.Id.ToString(),
            CollectionId = ChatConstants.MessagesCollectionName,
            CollectionName = ChatConstants.MessagesCollectionName,
            Created = message.Created,
            Updated = message.Updated,
            ChatId = message.ChatId.ToString(),
            Content = message.Content,
            IsAdmin = message.IsAdmin,
        };
    }
}
