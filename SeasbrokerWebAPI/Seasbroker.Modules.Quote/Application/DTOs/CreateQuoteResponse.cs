using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Quote.Application.DTOs;

public class CreateQuoteResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = "Quote request created successfully!";

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("requestedQuoteId")]
    public string RequestedQuoteId { get; set; } = string.Empty;
}
