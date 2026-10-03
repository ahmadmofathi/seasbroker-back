using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Quote.Application.DTOs;

public class TrackRequestDto
{
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

/// <summary>What a customer sees when they look up their request on the public tracking page.</summary>
public class RequestTrackingDto
{
    [JsonPropertyName("trackingNumber")]
    public string TrackingNumber { get; set; } = string.Empty;

    /// <summary>"Cargo Brokerage", "Ship Brokerage", "Customs Clearance" or "Request".</summary>
    [JsonPropertyName("service")]
    public string Service { get; set; } = string.Empty;

    [JsonPropertyName("submittedAt")]
    public DateTime SubmittedAt { get; set; }

    /// <summary>Cargo type, or vessel type for ship requests.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("departurePort")]
    public string? DeparturePort { get; set; }

    [JsonPropertyName("arrivalPort")]
    public string? ArrivalPort { get; set; }

    /// <summary>Machine key of the current stage, e.g. "review", "listed", "matched".</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("statusLabel")]
    public string StatusLabel { get; set; } = string.Empty;

    [JsonPropertyName("statusDetail")]
    public string StatusDetail { get; set; } = string.Empty;

    /// <summary>The stages of this kind of request, in order, with how far it has got.</summary>
    [JsonPropertyName("steps")]
    public List<TrackingStepDto> Steps { get; set; } = new();

    /// <summary>True while the customer can still change the answers they gave (the team hasn't accepted the request yet).</summary>
    [JsonPropertyName("canEdit")]
    public bool CanEdit { get; set; }

    /// <summary>The cargo listing reference (CRG-...), once the request has been listed.</summary>
    [JsonPropertyName("cargoReference")]
    public string? CargoReference { get; set; }

    /// <summary>The vessel's name, once a ship request has been added to the fleet.</summary>
    [JsonPropertyName("vesselName")]
    public string? VesselName { get; set; }
}

public class TrackingStepDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("current")]
    public bool Current { get; set; }
}
