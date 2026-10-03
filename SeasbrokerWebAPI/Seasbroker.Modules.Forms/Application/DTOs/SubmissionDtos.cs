using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Forms.Application.DTOs;

public class SubmitFormResponse
{
    [JsonPropertyName("submissionId")]
    public string SubmissionId { get; set; } = string.Empty;

    [JsonPropertyName("requestedQuoteId")]
    public string? RequestedQuoteId { get; set; }

    /// <summary>What the customer uses, with their email, to follow the request on the tracking page.</summary>
    [JsonPropertyName("trackingNumber")]
    public string? TrackingNumber { get; set; }
}

/// <summary>Identifies a request for editing: its tracking number and the email it was registered with.</summary>
public class RequestEditLookupDto
{
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public class RequestEditSaveDto : RequestEditLookupDto
{
    /// <summary>The answers, keyed by field key, in the same shape as when registering.</summary>
    [JsonPropertyName("values")]
    public Dictionary<string, System.Text.Json.JsonElement> Values { get; set; } = new();
}

/// <summary>A customer's own request, ready to be shown in the form it was registered with.</summary>
public class RequestEditFormDto
{
    [JsonPropertyName("trackingNumber")]
    public string TrackingNumber { get; set; } = string.Empty;

    /// <summary>The form as it was when the request was registered.</summary>
    [JsonPropertyName("schema")]
    public FormSchemaDto Schema { get; set; } = new();

    /// <summary>The current answers by field key: strings, booleans (checkboxes), string lists (multi-select) or a route.</summary>
    [JsonPropertyName("values")]
    public Dictionary<string, object?> Values { get; set; } = new();

    /// <summary>Files already attached. They stay as they are when the request is edited.</summary>
    [JsonPropertyName("files")]
    public List<RequestEditFileDto> Files { get; set; } = new();
}

public class RequestEditFileDto
{
    [JsonPropertyName("fieldKey")]
    public string FieldKey { get; set; } = string.Empty;

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("sizeBytes")]
    public long SizeBytes { get; set; }
}
