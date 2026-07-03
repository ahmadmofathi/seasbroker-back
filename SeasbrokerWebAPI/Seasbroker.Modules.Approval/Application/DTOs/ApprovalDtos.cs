using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Approval.Application.DTOs;

public class MatchApprovalRecordDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("collectionId")]
    public string CollectionId { get; set; } = string.Empty;

    [JsonPropertyName("collectionName")]
    public string CollectionName { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("updated")]
    public DateTime Updated { get; set; }

    [JsonPropertyName("cargoListingId")]
    public string CargoListingId { get; set; } = string.Empty;

    [JsonPropertyName("vesselId")]
    public string VesselId { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public decimal Score { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("matchReason")]
    public string MatchReason { get; set; } = string.Empty;

    [JsonPropertyName("scoreBreakdown")]
    public string? ScoreBreakdown { get; set; }

    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("chatId")]
    public string? ChatId { get; set; }

    [JsonPropertyName("approvedBy")]
    public string? ApprovedBy { get; set; }

    [JsonPropertyName("approvedAt")]
    public DateTime? ApprovedAt { get; set; }

    [JsonPropertyName("rejectedBy")]
    public string? RejectedBy { get; set; }

    [JsonPropertyName("rejectedAt")]
    public DateTime? RejectedAt { get; set; }

    [JsonPropertyName("cancelledBy")]
    public string? CancelledBy { get; set; }

    [JsonPropertyName("cancelledAt")]
    public DateTime? CancelledAt { get; set; }

    [JsonPropertyName("completedBy")]
    public string? CompletedBy { get; set; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("rowVersion")]
    public string RowVersion { get; set; } = string.Empty;
}

public class PocketBaseListResponse<T>
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
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
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

public class MatchApprovalActionRequest
{
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("rowVersion")]
    public string? RowVersion { get; set; }
}
