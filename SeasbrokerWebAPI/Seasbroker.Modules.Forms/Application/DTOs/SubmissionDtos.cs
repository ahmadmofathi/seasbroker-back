using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Forms.Application.DTOs;

public class SubmitFormResponse
{
    [JsonPropertyName("submissionId")]
    public string SubmissionId { get; set; } = string.Empty;

    [JsonPropertyName("requestedQuoteId")]
    public string? RequestedQuoteId { get; set; }
}
