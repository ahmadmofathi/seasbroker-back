using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Cargo.Application.DTOs;

public class CreateCargoListingRequest
{
    [JsonPropertyName("customer")]
    public string Customer { get; set; } = string.Empty;

    [JsonPropertyName("requestedQuote")]
    public string? RequestedQuote { get; set; }

    [JsonPropertyName("referenceNumber")]
    public string? ReferenceNumber { get; set; }

    [JsonPropertyName("cargoType")]
    public string CargoType { get; set; } = string.Empty;

    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    [JsonPropertyName("dimensions")]
    public string Dimensions { get; set; } = string.Empty;

    [JsonPropertyName("departurePort")]
    public string DeparturePort { get; set; } = string.Empty;

    [JsonPropertyName("departureTime")]
    public DateTime DepartureTime { get; set; }

    [JsonPropertyName("arrivalPort")]
    public string ArrivalPort { get; set; } = string.Empty;

    [JsonPropertyName("arrivalTime")]
    public DateTime ArrivalTime { get; set; }

    [JsonPropertyName("additionalInfo")]
    public string? AdditionalInfo { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }
}
