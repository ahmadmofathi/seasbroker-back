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

    /// <summary>Optional ordered route; the first stop is the vessel's next port. When given, it
    /// replaces openPort/destinationPort (they're set from the first and last stop).</summary>
    [JsonPropertyName("routeStops")]
    public List<RouteStopDto>? RouteStops { get; set; }
}
