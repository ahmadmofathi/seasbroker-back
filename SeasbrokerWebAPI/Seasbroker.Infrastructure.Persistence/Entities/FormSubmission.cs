namespace Seasbroker.Infrastructure.Persistence.Entities;

public class FormSubmission : AuditableEntity
{
    public Guid FormVersionId { get; set; }

    public FormVersion FormVersion { get; set; } = null!;

    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    /// <summary>The real business record this submission produced, if the form maps to one.</summary>
    public Guid? RequestedQuoteId { get; set; }

    public RequestedQuote? RequestedQuote { get; set; }

    public ICollection<FormSubmissionValue> Values { get; set; } = new List<FormSubmissionValue>();

    public ICollection<FormSubmissionFile> Files { get; set; } = new List<FormSubmissionFile>();
}
