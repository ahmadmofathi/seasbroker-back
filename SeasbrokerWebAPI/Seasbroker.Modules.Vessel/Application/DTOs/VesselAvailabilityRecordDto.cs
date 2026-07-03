using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Vessel.Application.DTOs;

public class VesselAvailabilityRecordDto
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

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}
