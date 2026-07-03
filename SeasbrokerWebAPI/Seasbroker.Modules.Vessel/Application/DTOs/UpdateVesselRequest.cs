using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Vessel.Application.DTOs;

public class UpdateVesselRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("vesselType")]
    public string? VesselType { get; set; }

    [JsonPropertyName("dwt")]
    public double? Dwt { get; set; }

    [JsonPropertyName("teuCapacity")]
    public int? TeuCapacity { get; set; }

    [JsonPropertyName("lengthOverall")]
    public double? LengthOverall { get; set; }

    [JsonPropertyName("beam")]
    public double? Beam { get; set; }

    [JsonPropertyName("draft")]
    public double? Draft { get; set; }

    [JsonPropertyName("currentPort")]
    public string? CurrentPort { get; set; }

    [JsonPropertyName("flagCountry")]
    public string? FlagCountry { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("customer")]
    public string? Customer { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
