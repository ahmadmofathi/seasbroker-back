using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Notifications.Application.DTOs;

public class NotificationDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("notificationType")]
    public string NotificationType { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("readAt")]
    public DateTime? ReadAt { get; set; }

    [JsonPropertyName("payload")]
    public string? Payload { get; set; }
}

public class NotificationListResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("perPage")]
    public int PerPage { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("items")]
    public IReadOnlyList<NotificationDto> Items { get; set; } = Array.Empty<NotificationDto>();
}

public class PocketBaseErrorResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("data")]
    public object Data { get; set; } = new { };
}

public class RealtimeNotificationDto
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = "create";

    [JsonPropertyName("record")]
    public NotificationDto Record { get; set; } = null!;
}
