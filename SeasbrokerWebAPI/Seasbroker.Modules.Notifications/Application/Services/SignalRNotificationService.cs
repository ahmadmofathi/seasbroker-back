using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Seasbroker.Modules.Notifications.Application.Constants;
using Seasbroker.Modules.Notifications.Application.DTOs;
using Seasbroker.Modules.Notifications.Application.Mapping;
using Seasbroker.Modules.Notifications.Hubs;

namespace Seasbroker.Modules.Notifications.Application.Services;

public interface ISignalRNotificationService
{
    Task PushAsync(NotificationDto notification, CancellationToken cancellationToken = default);

    Task PushManyAsync(
        IReadOnlyList<NotificationDto> notifications,
        CancellationToken cancellationToken = default);
}

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task PushAsync(NotificationDto notification, CancellationToken cancellationToken = default) =>
        PushManyAsync([notification], cancellationToken);

    public async Task PushManyAsync(
        IReadOnlyList<NotificationDto> notifications,
        CancellationToken cancellationToken = default)
    {
        foreach (var notification in notifications)
        {
            try
            {
                var payload = new RealtimeNotificationDto
                {
                    Action = "create",
                    Record = notification,
                };

                if (Guid.TryParse(notification.UserId, out var userId))
                {
                    await _hubContext.Clients
                        .Group(NotificationHubGroups.ForUser(userId))
                        .SendAsync(NotificationHubMethods.ReceiveNotification, payload, cancellationToken);
                }

                await _hubContext.Clients
                    .Group(NotificationHubGroups.Admin)
                    .SendAsync(NotificationHubMethods.ReceiveNotification, payload, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "SignalR delivery failed for notification {NotificationId}.",
                    notification.Id);
            }
        }
    }
}
