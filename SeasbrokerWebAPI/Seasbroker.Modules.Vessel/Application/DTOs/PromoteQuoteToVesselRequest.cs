using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Vessel.Application.DTOs;

public class PromoteQuoteToVesselRequest
{
    [JsonPropertyName("requestedQuoteId")]
    public string RequestedQuoteId { get; set; } = string.Empty;
}
