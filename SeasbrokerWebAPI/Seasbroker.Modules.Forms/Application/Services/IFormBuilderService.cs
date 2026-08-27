using Seasbroker.Modules.Forms.Application.DTOs;

namespace Seasbroker.Modules.Forms.Application.Services;

public interface IFormBuilderService
{
    Task<IReadOnlyList<FormSummaryDto>> ListFormsAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the draft version, creating one cloned from the published version if none exists.</summary>
    Task<FormSchemaDto> GetDraftAsync(string formKey, CancellationToken cancellationToken = default);

    Task<FormSchemaDto> SaveDraftAsync(string formKey, FormSchemaDto schema, CancellationToken cancellationToken = default);

    Task<FormSchemaDto> PublishDraftAsync(string formKey, CancellationToken cancellationToken = default);

    /// <summary>Null if the form has never been published (shouldn't happen once seeded).</summary>
    Task<FormSchemaDto?> GetPublishedSchemaAsync(string formKey, CancellationToken cancellationToken = default);
}
