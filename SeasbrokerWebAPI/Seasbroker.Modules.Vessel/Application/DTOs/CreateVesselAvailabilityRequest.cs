using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Vessel.Application.DTOs;

public class CreateVesselAvailabilityRequest
{
    [JsonPropertyName("vesselId")]
    public string VesselId { get; set; } = string.Empty;

    [JsonPropertyName("availableFrom")]
    public DateTime AvailableFrom { get; set; }

    [JsonPropertyName("availableTo")]
    public DateTime AvailableTo { get; set; }

    [JsonPropertyName("openPort")]
    public string OpenPort { get; set; } = string.Empty;

    [JsonPropertyName("destinationPort")]
    public string? DestinationPort { get; set; }
}
