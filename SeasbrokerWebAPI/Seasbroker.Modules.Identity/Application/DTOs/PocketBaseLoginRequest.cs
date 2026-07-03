using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Identity.Application.DTOs;

public class PocketBaseLoginRequest
{
    [JsonPropertyName("identity")]
    public string Identity { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
