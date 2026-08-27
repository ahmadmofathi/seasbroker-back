namespace Seasbroker.Infrastructure.Persistence.Entities;

public class FormSection : AuditableEntity
{
    public Guid FormVersionId { get; set; }

    public FormVersion FormVersion { get; set; } = null!;

    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int Order { get; set; }

    public bool Visible { get; set; } = true;

    public ICollection<FormField> Fields { get; set; } = new List<FormField>();
}
