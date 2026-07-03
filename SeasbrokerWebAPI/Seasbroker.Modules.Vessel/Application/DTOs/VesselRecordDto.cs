using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Vessel.Application.DTOs;

public class VesselRecordDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("collectionId")]
    public string CollectionId { get; set; } = string.Empty;

    [JsonPropertyName("collectionName")]
    public string CollectionName { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("updated")]
    public DateTime Updated { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("imoNumber")]
    public string? ImoNumber { get; set; }

    [JsonPropertyName("vesselType")]
    public string VesselType { get; set; } = string.Empty;

    [JsonPropertyName("dwt")]
    public double Dwt { get; set; }

    [JsonPropertyName("teuCapacity")]
    public int? TeuCapacity { get; set; }

    [JsonPropertyName("lengthOverall")]
    public double? LengthOverall { get; set; }

    [JsonPropertyName("beam")]
    public double? Beam { get; set; }

    [JsonPropertyName("draft")]
    public double? Draft { get; set; }

    [JsonPropertyName("currentPort")]
    public string CurrentPort { get; set; } = string.Empty;

    [JsonPropertyName("flagCountry")]
    public string? FlagCountry { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("customer")]
    public string? Customer { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
