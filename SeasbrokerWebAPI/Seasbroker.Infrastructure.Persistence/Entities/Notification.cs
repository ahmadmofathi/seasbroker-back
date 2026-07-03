namespace Seasbroker.Infrastructure.Persistence.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string NotificationType { get; set; } = string.Empty;

    public string Status { get; set; } = NotificationStatus.Unread;

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public string? Payload { get; set; }
}
