namespace Seasbroker.Infrastructure.Persistence.Entities;

public class FormSubmissionFile : AuditableEntity
{
    public Guid FormSubmissionId { get; set; }

    public FormSubmission FormSubmission { get; set; } = null!;

    public string FieldKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    /// <summary>Path relative to the configured storage root - never served as a static file.</summary>
    public string StoragePath { get; set; } = string.Empty;
}
