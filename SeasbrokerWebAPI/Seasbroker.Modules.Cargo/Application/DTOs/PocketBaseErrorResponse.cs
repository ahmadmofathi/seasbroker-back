using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Cargo.Application.DTOs;

public class PocketBaseErrorResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("data")]
    public object Data { get; set; } = new { };
}
