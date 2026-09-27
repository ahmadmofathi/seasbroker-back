using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Matching.Application.DTOs;

public class MatchRecordDto
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

    // Who the match is between, so the admin sees names instead of ids. Filled when the query
    // loads the cargo listing / vessel (the match lists do); null otherwise.

    [JsonPropertyName("cargoReference")]
    public string? CargoReference { get; set; }

    [JsonPropertyName("cargoType")]
    public string? CargoType { get; set; }

    [JsonPropertyName("cargoRoute")]
    public string? CargoRoute { get; set; }

    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("vesselName")]
    public string? VesselName { get; set; }

    [JsonPropertyName("vesselType")]
    public string? VesselType { get; set; }

    [JsonPropertyName("vesselDwt")]
    public double? VesselDwt { get; set; }

    [JsonPropertyName("vesselImo")]
    public string? VesselImo { get; set; }
}
