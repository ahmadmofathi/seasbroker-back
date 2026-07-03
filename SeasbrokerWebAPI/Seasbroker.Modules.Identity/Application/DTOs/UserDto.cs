using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Identity.Application.DTOs;

public class UserDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("roles")]
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("updated")]
    public DateTime Updated { get; set; }
}
