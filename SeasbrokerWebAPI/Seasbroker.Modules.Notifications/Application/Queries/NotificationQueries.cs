namespace Seasbroker.Modules.Notifications.Application.Queries;

public sealed record GetNotificationsQuery(Guid UserId, int Page = 1, int PerPage = 50);

public sealed record GetUnreadNotificationsQuery(Guid UserId, int Page = 1, int PerPage = 50);

public sealed record MarkNotificationReadCommand(Guid UserId, Guid NotificationId);

public sealed record MarkAllNotificationsReadCommand(Guid UserId);

public sealed record DeleteNotificationCommand(Guid UserId, Guid NotificationId);

public sealed record CreateNotificationRequest(
    Guid UserId,
    string Title,
    string Message,
    string NotificationType,
    string? Payload);
