namespace Seasbroker.Infrastructure.Persistence.Entities;

/// <summary>
/// A versioned snapshot of a form's structure. Exactly one Published version exists per
/// FormDefinition at a time; at most one Draft. Submissions pin to the version that was live
/// when they were submitted, so historical submissions never change shape underneath the admin.
/// </summary>
public class FormVersion : AuditableEntity
{
    public Guid FormDefinitionId { get; set; }

    public FormDefinition FormDefinition { get; set; } = null!;

    public int VersionNumber { get; set; }

    public string Status { get; set; } = FormVersionStatus.Draft;

    public DateTime? PublishedAt { get; set; }

    public ICollection<FormSection> Sections { get; set; } = new List<FormSection>();
}
