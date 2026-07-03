using Seasbroker.Modules.Chat.Application.DTOs;

namespace Seasbroker.Modules.Chat.Application.Abstractions;

public interface IChatNotificationService
{
    Task NotifyChatCreatedAsync(ChatRecordDto chat, CancellationToken cancellationToken = default);

    Task NotifyMessageCreatedAsync(MessageRecordDto message, CancellationToken cancellationToken = default);
}
