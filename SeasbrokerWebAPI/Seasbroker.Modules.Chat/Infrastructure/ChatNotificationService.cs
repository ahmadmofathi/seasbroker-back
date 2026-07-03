using Microsoft.AspNetCore.SignalR;
using Seasbroker.Modules.Chat.Application.Abstractions;
using Seasbroker.Modules.Chat.Application.Constants;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Hubs;

namespace Seasbroker.Modules.Chat.Infrastructure;

public class ChatNotificationService : IChatNotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatNotificationService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyChatCreatedAsync(ChatRecordDto chat, CancellationToken cancellationToken = default)
    {
        var payload = new RealtimeEventDto<ChatRecordDto>
        {
            Action = "create",
            Record = chat,
        };

        await _hubContext.Clients
            .Group(ChatNotificationGroups.AdminChats)
            .SendAsync("ReceiveChatEvent", payload, cancellationToken);
    }

    public async Task NotifyMessageCreatedAsync(MessageRecordDto message, CancellationToken cancellationToken = default)
    {
        var payload = new RealtimeEventDto<MessageRecordDto>
        {
            Action = "create",
            Record = message,
        };

        await _hubContext.Clients
            .Group(ChatNotificationGroups.AdminMessages)
            .SendAsync("ReceiveMessageEvent", payload, cancellationToken);

        await _hubContext.Clients
            .Group(ChatNotificationGroups.ForChat(message.ChatId))
            .SendAsync("ReceiveMessageEvent", payload, cancellationToken);
    }
}
