using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Notifications.Application.DTOs;

namespace Seasbroker.Modules.Notifications.Application.Mapping;

public static class NotificationMapper
{
    public static NotificationDto ToDto(Notification notification) =>
        new()
        {
            Id = notification.Id.ToString(),
            UserId = notification.UserId.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            NotificationType = notification.NotificationType,
            Status = notification.Status,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            Payload = notification.Payload,
        };
}
