using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Identity.Application.DTOs;

public class PocketBaseAuthResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("record")]
    public PocketBaseSuperuserRecord Record { get; set; } = null!;
}
