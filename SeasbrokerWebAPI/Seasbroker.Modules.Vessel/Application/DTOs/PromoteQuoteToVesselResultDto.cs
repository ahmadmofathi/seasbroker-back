using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Vessel.Application.DTOs;

public class PromoteQuoteToVesselResultDto
{
    [JsonPropertyName("vessel")]
    public VesselRecordDto Vessel { get; set; } = new();

    /// <summary>The availability window created from the request, or null when none could be.</summary>
    [JsonPropertyName("availability")]
    public VesselAvailabilityRecordDto? Availability { get; set; }

    /// <summary>Why no availability was created, when it wasn't.</summary>
    [JsonPropertyName("availabilityNote")]
    public string? AvailabilityNote { get; set; }

    /// <summary>Reference of a cargo listing wrongly created from this ship request earlier, now cancelled.</summary>
    [JsonPropertyName("cancelledCargoListingReference")]
    public string? CancelledCargoListingReference { get; set; }
}
