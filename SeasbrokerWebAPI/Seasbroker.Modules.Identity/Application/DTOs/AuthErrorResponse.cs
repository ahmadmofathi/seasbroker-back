using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Identity.Application.DTOs;

public class AuthErrorResponse
{
    [JsonPropertyName("data")]
    public object Data { get; set; } = new { };

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }
}
