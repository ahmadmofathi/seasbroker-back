using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Matching.Application.DTOs;

public class MatchingRunResultDto
{
    [JsonPropertyName("matchesCreated")]
    public int MatchesCreated { get; set; }

    [JsonPropertyName("matchesSkipped")]
    public int MatchesSkipped { get; set; }

    [JsonPropertyName("items")]
    public IReadOnlyList<MatchRecordDto> Items { get; set; } = Array.Empty<MatchRecordDto>();
}
