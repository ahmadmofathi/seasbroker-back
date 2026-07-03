namespace Seasbroker.Infrastructure.Persistence.Entities;

public class Message : AuditableEntity
{
    public Guid ChatId { get; set; }

    public Chat Chat { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
}
