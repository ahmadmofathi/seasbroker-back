using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Matching.Application.DTOs;

public class CreateManualMatchRequest
{
    [JsonPropertyName("cargoListingId")]
    public string CargoListingId { get; set; } = string.Empty;

    [JsonPropertyName("vesselId")]
    public string VesselId { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public decimal? Score { get; set; }

    [JsonPropertyName("matchReason")]
    public string? MatchReason { get; set; }
}
