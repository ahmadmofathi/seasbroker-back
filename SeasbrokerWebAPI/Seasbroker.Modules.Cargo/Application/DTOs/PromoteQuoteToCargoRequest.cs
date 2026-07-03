using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Cargo.Application.DTOs;

public class PromoteQuoteToCargoRequest
{
    [JsonPropertyName("requestedQuoteId")]
    public string RequestedQuoteId { get; set; } = string.Empty;

    [JsonPropertyName("referenceNumber")]
    public string? ReferenceNumber { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }
}
