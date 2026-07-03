using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Matching.Application.DTOs;

public class RunMatchingRequest
{
    [JsonPropertyName("cargoListingId")]
    public string? CargoListingId { get; set; }

    [JsonPropertyName("vesselId")]
    public string? VesselId { get; set; }
}
