namespace Seasbroker.Infrastructure.Persistence.Entities;

public class ChatToken : AuditableEntity
{
    public string Token { get; set; } = string.Empty;

    public Guid ChatId { get; set; }

    public Chat Chat { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
}
