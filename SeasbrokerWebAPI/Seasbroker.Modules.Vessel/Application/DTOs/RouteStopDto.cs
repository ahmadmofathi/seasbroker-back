using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Vessel.Application.DTOs;

/// <summary>One port call on a vessel's route, in sailing order.</summary>
public class RouteStopDto
{
    [JsonPropertyName("port")]
    public string Port { get; set; } = string.Empty;

    [JsonPropertyName("eta")]
    public DateTime Eta { get; set; }
}
