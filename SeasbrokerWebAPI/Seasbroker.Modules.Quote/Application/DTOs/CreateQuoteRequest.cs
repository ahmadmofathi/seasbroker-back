using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Quote.Application.DTOs;

public class CreateQuoteRequest
{
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

    [JsonPropertyName("fname")]
    public string Fname { get; set; } = string.Empty;

    [JsonPropertyName("lname")]
    public string Lname { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;
}
