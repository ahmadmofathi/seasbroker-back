namespace Seasbroker.Infrastructure.Persistence.Entities;

/// <summary>
/// One of the fixed set of public request forms (Request Quote, Request Route, Request Clearance).
/// Admins never create new FormDefinitions through the UI - they configure the versions of these.
/// </summary>
public class FormDefinition : AuditableEntity
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<FormVersion> Versions { get; set; } = new List<FormVersion>();
}
