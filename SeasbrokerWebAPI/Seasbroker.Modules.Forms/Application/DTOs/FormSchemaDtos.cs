using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Forms.Application.DTOs;

public class FormSummaryDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("publishedVersionNumber")]
    public int? PublishedVersionNumber { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTime? PublishedAt { get; set; }

    [JsonPropertyName("draftVersionNumber")]
    public int? DraftVersionNumber { get; set; }

    [JsonPropertyName("hasUnpublishedDraft")]
    public bool HasUnpublishedDraft { get; set; }
}

public class FormSchemaDto
{
    [JsonPropertyName("formKey")]
    public string FormKey { get; set; } = string.Empty;

    [JsonPropertyName("versionNumber")]
    public int VersionNumber { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("sections")]
    public List<FormSectionDto> Sections { get; set; } = new();
}

public class FormSectionDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("visible")]
    public bool Visible { get; set; } = true;

    [JsonPropertyName("fields")]
    public List<FormFieldDto> Fields { get; set; } = new();
}

public class FormFieldDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    [JsonPropertyName("helpText")]
    public string? HelpText { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonPropertyName("visible")]
    public bool Visible { get; set; } = true;

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("width")]
    public string Width { get; set; } = "Full";

    [JsonPropertyName("isSystemField")]
    public bool IsSystemField { get; set; }

    [JsonPropertyName("systemFieldKey")]
    public string? SystemFieldKey { get; set; }

    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; set; }

    [JsonPropertyName("options")]
    public List<FormFieldOptionDto> Options { get; set; } = new();

    [JsonPropertyName("validation")]
    public FormFieldValidationDto? Validation { get; set; }

    [JsonPropertyName("conditionCombinator")]
    public string? ConditionCombinator { get; set; }

    [JsonPropertyName("conditions")]
    public List<FormFieldConditionDto> Conditions { get; set; } = new();
}

public class FormFieldOptionDto
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public int Order { get; set; }
}

public class FormFieldValidationDto
{
    [JsonPropertyName("minLength")]
    public int? MinLength { get; set; }

    [JsonPropertyName("maxLength")]
    public int? MaxLength { get; set; }

    [JsonPropertyName("min")]
    public double? Min { get; set; }

    [JsonPropertyName("max")]
    public double? Max { get; set; }

    [JsonPropertyName("pattern")]
    public string? Pattern { get; set; }

    /// <summary>When true, the input strips non-digit keystrokes live instead of only rejecting them on submit.</summary>
    [JsonPropertyName("digitsOnly")]
    public bool? DigitsOnly { get; set; }

    /// <summary>Date/DateTime only: rejects a date earlier than today (UTC date, ignoring time-of-day).</summary>
    [JsonPropertyName("noPastDates")]
    public bool? NoPastDates { get; set; }

    /// <summary>Number/Decimal only: permits a leading minus sign. Off by default - most numeric
    /// fields (weights, counts, capacities) can never be negative; only set this for fields that
    /// genuinely can be (e.g. sub-zero temperatures).</summary>
    [JsonPropertyName("allowNegative")]
    public bool? AllowNegative { get; set; }

    /// <summary>A 4-digit year field: rejects a year later than the current year.</summary>
    [JsonPropertyName("noFutureYear")]
    public bool? NoFutureYear { get; set; }

    [JsonPropertyName("fileMaxSizeMB")]
    public double? FileMaxSizeMB { get; set; }

    [JsonPropertyName("allowedExtensions")]
    public List<string>? AllowedExtensions { get; set; }

    [JsonPropertyName("minSelections")]
    public int? MinSelections { get; set; }

    [JsonPropertyName("maxSelections")]
    public int? MaxSelections { get; set; }
}

public class FormFieldConditionDto
{
    [JsonPropertyName("sourceFieldKey")]
    public string SourceFieldKey { get; set; } = string.Empty;

    [JsonPropertyName("operator")]
    public string Operator { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
