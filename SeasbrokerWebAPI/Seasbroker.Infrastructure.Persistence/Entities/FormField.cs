namespace Seasbroker.Infrastructure.Persistence.Entities;

public class FormField : AuditableEntity
{
    public Guid FormVersionId { get; set; }

    public FormVersion FormVersion { get; set; } = null!;

    public Guid FormSectionId { get; set; }

    public FormSection FormSection { get; set; } = null!;

    /// <summary>Stable key, independent of Label. Never changes after creation.</summary>
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Type { get; set; } = FormFieldType.Text;

    public string? Placeholder { get; set; }

    public string? HelpText { get; set; }

    public bool Required { get; set; }

    public bool Visible { get; set; } = true;

    public int Order { get; set; }

    public string Width { get; set; } = FormFieldWidth.Full;

    /// <summary>True for a field tied to an existing business concept (Cargo Type, Email, ...).</summary>
    public bool IsSystemField { get; set; }

    /// <summary>
    /// Stable business identifier this field maps to (e.g. "CargoType", "Email").
    /// Null for admin-created custom fields. Never destroyed by relabeling/reordering.
    /// </summary>
    public string? SystemFieldKey { get; set; }

    public string? DefaultValue { get; set; }

    /// <summary>Structured validation config (min/max/length/pattern/file constraints) - never code.</summary>
    public string? ValidationJson { get; set; }

    /// <summary>AND / OR across this field's Conditions. Null when the field has no conditions.</summary>
    public string? ConditionCombinator { get; set; }

    public ICollection<FormFieldOption> Options { get; set; } = new List<FormFieldOption>();

    public ICollection<FormFieldCondition> Conditions { get; set; } = new List<FormFieldCondition>();
}
