namespace Seasbroker.Infrastructure.Persistence.Entities;

public abstract class AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }
}
