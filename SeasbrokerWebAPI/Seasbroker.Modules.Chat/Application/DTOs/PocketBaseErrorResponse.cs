using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Chat.Application.DTOs;

public class PocketBaseErrorResponse
{
    [JsonPropertyName("data")]
    public object Data { get; set; } = new { };

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }
}
