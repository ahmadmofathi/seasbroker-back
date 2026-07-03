namespace Seasbroker.Infrastructure.Persistence.Entities;

public class Customer : AuditableEntity
{
    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public ICollection<RequestedQuote> RequestedQuotes { get; set; } = new List<RequestedQuote>();
}
