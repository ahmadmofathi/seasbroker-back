namespace Seasbroker.Infrastructure.Persistence.Entities;

public class MatchingRule : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Criterion { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Configuration { get; set; }
}
