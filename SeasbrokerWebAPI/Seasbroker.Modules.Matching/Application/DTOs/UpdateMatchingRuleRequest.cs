using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Matching.Application.DTOs;

public class UpdateMatchingRuleRequest
{
    [JsonPropertyName("weight")]
    public decimal? Weight { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("configuration")]
    public string? Configuration { get; set; }
}
