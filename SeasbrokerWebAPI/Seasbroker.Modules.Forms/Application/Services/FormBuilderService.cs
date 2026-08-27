using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Exceptions;
using Seasbroker.Modules.Forms.Application.Mapping;

namespace Seasbroker.Modules.Forms.Application.Services;

public class FormBuilderService : IFormBuilderService
{
    private readonly SeasbrokerDbContext _dbContext;

    public FormBuilderService(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FormSummaryDto>> ListFormsAsync(CancellationToken cancellationToken = default)
    {
        var definitions = await _dbContext.FormDefinitions
            .AsNoTracking()
            .Include(f => f.Versions)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

        return definitions.Select(def =>
        {
            var published = def.Versions.Where(v => v.Status == FormVersionStatus.Published)
                .OrderByDescending(v => v.VersionNumber).FirstOrDefault();
            var draft = def.Versions.Where(v => v.Status == FormVersionStatus.Draft)
                .OrderByDescending(v => v.VersionNumber).FirstOrDefault();

            return new FormSummaryDto
            {
                Key = def.Key,
                Name = def.Name,
                Description = def.Description,
                PublishedVersionNumber = published?.VersionNumber,
                PublishedAt = published?.PublishedAt,
                DraftVersionNumber = draft?.VersionNumber,
                HasUnpublishedDraft = draft is not null,
            };
        }).ToList();
    }

    public async Task<FormSchemaDto> GetDraftAsync(string formKey, CancellationToken cancellationToken = default)
    {
        var definition = await GetDefinitionAsync(formKey, cancellationToken);

        var draft = await LoadVersionAsync(definition.Id, FormVersionStatus.Draft, cancellationToken);
        if (draft is not null)
        {
            return FormMapper.ToSchemaDto(draft, formKey);
        }

        var published = await LoadVersionAsync(definition.Id, FormVersionStatus.Published, cancellationToken)
            ?? throw new FormsException($"Form '{formKey}' has no published version to draft from.", StatusCodes.Status409Conflict);

        var publishedDto = FormMapper.ToSchemaDto(published, formKey);
        var nextVersionNumber = await NextVersionNumberAsync(definition.Id, cancellationToken);
        var newDraft = FormMapper.ToNewVersion(definition.Id, nextVersionNumber, FormVersionStatus.Draft, publishedDto);

        _dbContext.FormVersions.Add(newDraft);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return FormMapper.ToSchemaDto(newDraft, formKey);
    }

    public async Task<FormSchemaDto> SaveDraftAsync(string formKey, FormSchemaDto schema, CancellationToken cancellationToken = default)
    {
        FormSchemaValidator.Validate(schema);

        var definition = await GetDefinitionAsync(formKey, cancellationToken);
        var draft = await LoadVersionAsync(definition.Id, FormVersionStatus.Draft, cancellationToken)
            ?? throw new FormsException($"Form '{formKey}' has no draft. Load the draft before saving.", StatusCodes.Status409Conflict);

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var existingSections = await _dbContext.FormSections
                .Where(s => s.FormVersionId == draft.Id)
                .ToListAsync(cancellationToken);
            _dbContext.FormSections.RemoveRange(existingSections);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var rebuilt = FormMapper.ToNewVersion(definition.Id, draft.VersionNumber, FormVersionStatus.Draft, schema);
            foreach (var section in rebuilt.Sections)
            {
                section.FormVersionId = draft.Id;
                foreach (var field in section.Fields)
                {
                    field.FormVersionId = draft.Id;
                }
            }

            _dbContext.FormSections.AddRange(rebuilt.Sections);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        var saved = await LoadVersionAsync(definition.Id, FormVersionStatus.Draft, cancellationToken);
        return FormMapper.ToSchemaDto(saved!, formKey);
    }

    public async Task<FormSchemaDto> PublishDraftAsync(string formKey, CancellationToken cancellationToken = default)
    {
        var definition = await GetDefinitionAsync(formKey, cancellationToken);

        var draft = await _dbContext.FormVersions
            .FirstOrDefaultAsync(v => v.FormDefinitionId == definition.Id && v.Status == FormVersionStatus.Draft, cancellationToken)
            ?? throw new FormsException($"Form '{formKey}' has no draft to publish.", StatusCodes.Status409Conflict);

        var currentPublished = await _dbContext.FormVersions
            .Where(v => v.FormDefinitionId == definition.Id && v.Status == FormVersionStatus.Published)
            .ToListAsync(cancellationToken);

        foreach (var version in currentPublished)
        {
            version.Status = FormVersionStatus.Archived;
        }

        draft.Status = FormVersionStatus.Published;
        draft.PublishedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var published = await LoadVersionAsync(definition.Id, FormVersionStatus.Published, cancellationToken);
        return FormMapper.ToSchemaDto(published!, formKey);
    }

    public async Task<FormSchemaDto?> GetPublishedSchemaAsync(string formKey, CancellationToken cancellationToken = default)
    {
        var definition = await _dbContext.FormDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Key == formKey, cancellationToken);

        if (definition is null)
        {
            return null;
        }

        var published = await LoadVersionAsync(definition.Id, FormVersionStatus.Published, cancellationToken);
        return published is null ? null : FormMapper.ToSchemaDto(published, formKey);
    }

    private async Task<FormDefinition> GetDefinitionAsync(string formKey, CancellationToken cancellationToken)
    {
        return await _dbContext.FormDefinitions.FirstOrDefaultAsync(f => f.Key == formKey, cancellationToken)
            ?? throw new FormsException($"Unknown form '{formKey}'.", StatusCodes.Status404NotFound);
    }

    private async Task<FormVersion?> LoadVersionAsync(Guid formDefinitionId, string status, CancellationToken cancellationToken)
    {
        return await _dbContext.FormVersions
            .AsNoTracking()
            .Include(v => v.Sections).ThenInclude(s => s.Fields).ThenInclude(f => f.Options)
            .Include(v => v.Sections).ThenInclude(s => s.Fields).ThenInclude(f => f.Conditions)
            .Where(v => v.FormDefinitionId == formDefinitionId && v.Status == status)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<int> NextVersionNumberAsync(Guid formDefinitionId, CancellationToken cancellationToken)
    {
        var max = await _dbContext.FormVersions
            .Where(v => v.FormDefinitionId == formDefinitionId)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync(cancellationToken);

        return (max ?? 0) + 1;
    }
}
