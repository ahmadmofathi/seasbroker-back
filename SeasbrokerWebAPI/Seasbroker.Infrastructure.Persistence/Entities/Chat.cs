namespace Seasbroker.Infrastructure.Persistence.Entities;

public class Chat : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public ICollection<ChatToken> ChatTokens { get; set; } = new List<ChatToken>();
}
