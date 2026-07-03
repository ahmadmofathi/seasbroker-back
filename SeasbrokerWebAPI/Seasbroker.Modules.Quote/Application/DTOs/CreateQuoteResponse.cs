using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Quote.Application.DTOs;

public class CreateQuoteResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = "Quote request created successfully!";
}
