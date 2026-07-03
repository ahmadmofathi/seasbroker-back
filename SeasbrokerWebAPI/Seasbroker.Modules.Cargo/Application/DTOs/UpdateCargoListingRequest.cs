using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Cargo.Application.DTOs;

public class UpdateCargoListingRequest
{
    [JsonPropertyName("cargoType")]
    public string? CargoType { get; set; }

    [JsonPropertyName("weight")]
    public double? Weight { get; set; }

    [JsonPropertyName("dimensions")]
    public string? Dimensions { get; set; }

    [JsonPropertyName("departurePort")]
    public string? DeparturePort { get; set; }

    [JsonPropertyName("departureTime")]
    public DateTime? DepartureTime { get; set; }

    [JsonPropertyName("arrivalPort")]
    public string? ArrivalPort { get; set; }

    [JsonPropertyName("arrivalTime")]
    public DateTime? ArrivalTime { get; set; }

    [JsonPropertyName("additionalInfo")]
    public string? AdditionalInfo { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }
}
