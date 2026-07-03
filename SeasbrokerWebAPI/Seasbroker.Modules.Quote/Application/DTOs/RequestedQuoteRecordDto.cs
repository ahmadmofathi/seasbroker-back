using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Quote.Application.DTOs;

public class RequestedQuoteRecordDto
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

    [JsonPropertyName("customer")]
    public string Customer { get; set; } = string.Empty;

    [JsonPropertyName("cargoType")]
    public string CargoType { get; set; } = string.Empty;

    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    [JsonPropertyName("departurePort")]
    public string DeparturePort { get; set; } = string.Empty;

    [JsonPropertyName("departureTime")]
    public string DepartureTime { get; set; } = string.Empty;

    [JsonPropertyName("arrivalPort")]
    public string ArrivalPort { get; set; } = string.Empty;

    [JsonPropertyName("arrivalTime")]
    public string ArrivalTime { get; set; } = string.Empty;

    [JsonPropertyName("dimensions")]
    public string Dimensions { get; set; } = string.Empty;

    [JsonPropertyName("additionalInfo")]
    public string? AdditionalInfo { get; set; }
}
