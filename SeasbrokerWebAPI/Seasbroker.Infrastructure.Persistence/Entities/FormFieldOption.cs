namespace Seasbroker.Infrastructure.Persistence.Entities;

public class FormFieldOption : AuditableEntity
{
    public Guid FormFieldId { get; set; }

    public FormField FormField { get; set; } = null!;

    public string Value { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int Order { get; set; }
}
