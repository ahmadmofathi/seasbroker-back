using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Identity.Application.DTOs;

public class LogoutRequest
{
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
}
