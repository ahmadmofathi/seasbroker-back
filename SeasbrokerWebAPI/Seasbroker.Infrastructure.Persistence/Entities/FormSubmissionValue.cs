namespace Seasbroker.Infrastructure.Persistence.Entities;

/// <summary>
/// EAV value for one field of one submission, keyed by the field's stable Key (not its label).
/// Covers both system and custom fields - there are no per-custom-field database columns.
/// </summary>
public class FormSubmissionValue : AuditableEntity
{
    public Guid FormSubmissionId { get; set; }

    public FormSubmission FormSubmission { get; set; } = null!;

    public string FieldKey { get; set; } = string.Empty;

    /// <summary>Raw value. Multi-value fields (MultiSelect/Checkbox group) store a JSON array string.</summary>
    public string? ValueText { get; set; }
}
