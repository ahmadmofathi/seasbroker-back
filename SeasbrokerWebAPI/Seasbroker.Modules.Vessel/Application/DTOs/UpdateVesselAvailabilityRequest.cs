using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Vessel.Application.DTOs;

public class UpdateVesselAvailabilityRequest
{
    [JsonPropertyName("availableFrom")]
    public DateTime? AvailableFrom { get; set; }

    [JsonPropertyName("availableTo")]
    public DateTime? AvailableTo { get; set; }

    [JsonPropertyName("openPort")]
    public string? OpenPort { get; set; }

    [JsonPropertyName("destinationPort")]
    public string? DestinationPort { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
}
