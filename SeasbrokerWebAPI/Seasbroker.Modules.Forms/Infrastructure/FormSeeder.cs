using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Mapping;
using Seasbroker.Modules.Forms.Application.Services;

namespace Seasbroker.Modules.Forms.Infrastructure;

/// <summary>
/// Seeds the 3 fixed forms with a published v1 schema reproducing the existing hard-coded
/// forms, the first time the app starts against a database that doesn't have them yet. Runs
/// once per missing form; never touches a form that's already been seeded (and possibly
/// since edited/published by an admin).
///
/// For forms that already exist it applies <see cref="FormSchemaUpgrades"/> to the published
/// version (and to an open draft, if there is one), keeping every admin edit. A new version is
/// published only when a patch actually changed something; the previous one is archived, not deleted.
/// </summary>
public class FormSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FormSeeder> _logger;

    public FormSeeder(IServiceScopeFactory scopeFactory, ILogger<FormSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SeasbrokerDbContext>();

        foreach (var (key, name, description, schema) in new[]
                 {
                     FormSeedData.RequestQuote(),
                     FormSeedData.RequestRoute(),
                     FormSeedData.RequestClearance(),
                 })
        {
            await SeedFormAsync(dbContext, key, name, description, schema, cancellationToken);

            try
            {
                await UpgradeFormAsync(dbContext, scope.ServiceProvider, key, cancellationToken);
            }
            catch (Exception ex)
            {
                // A failed upgrade must never stop the API from starting - the form just stays as it was.
                _logger.LogError(ex, "Couldn't upgrade form '{FormKey}'; it was left unchanged.", key);
                dbContext.ChangeTracker.Clear();
            }
        }
    }

    private async Task UpgradeFormAsync(
        SeasbrokerDbContext dbContext,
        IServiceProvider services,
        string key,
        CancellationToken cancellationToken)
    {
        var definition = await dbContext.FormDefinitions.FirstOrDefaultAsync(f => f.Key == key, cancellationToken);
        if (definition is null)
        {
            return;
        }

        var published = await LoadVersionAsync(dbContext, definition.Id, FormVersionStatus.Published, cancellationToken);
        if (published is not null)
        {
            var schema = FormMapper.ToSchemaDto(published, key);
            if (FormSchemaUpgrades.Apply(key, schema))
            {
                FormSchemaValidator.Validate(schema);

                var nextVersion = await dbContext.FormVersions
                    .Where(v => v.FormDefinitionId == definition.Id)
                    .MaxAsync(v => v.VersionNumber, cancellationToken) + 1;

                var currentlyPublished = await dbContext.FormVersions
                    .Where(v => v.FormDefinitionId == definition.Id && v.Status == FormVersionStatus.Published)
                    .ToListAsync(cancellationToken);
                foreach (var version in currentlyPublished)
                {
                    version.Status = FormVersionStatus.Archived;
                }

                var upgraded = FormMapper.ToNewVersion(definition.Id, nextVersion, FormVersionStatus.Published, schema);
                upgraded.PublishedAt = DateTime.UtcNow;
                dbContext.FormVersions.Add(upgraded);
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Upgraded form '{FormKey}': published v{Version} on top of v{Previous}.",
                    key, nextVersion, published.VersionNumber);
            }
        }

        // An admin's unpublished draft was copied from the old version - patch it too, so
        // publishing it later doesn't undo the upgrade. It stays a draft.
        var draft = await LoadVersionAsync(dbContext, definition.Id, FormVersionStatus.Draft, cancellationToken);
        if (draft is not null)
        {
            var draftSchema = FormMapper.ToSchemaDto(draft, key);
            if (FormSchemaUpgrades.Apply(key, draftSchema))
            {
                await services.GetRequiredService<IFormBuilderService>().SaveDraftAsync(key, draftSchema, cancellationToken);
                _logger.LogInformation("Upgraded the open draft of form '{FormKey}'.", key);
            }
        }
    }

    private static Task<FormVersion?> LoadVersionAsync(
        SeasbrokerDbContext dbContext,
        Guid formDefinitionId,
        string status,
        CancellationToken cancellationToken) =>
        dbContext.FormVersions
            .AsNoTracking()
            .Include(v => v.Sections).ThenInclude(s => s.Fields).ThenInclude(f => f.Options)
            .Include(v => v.Sections).ThenInclude(s => s.Fields).ThenInclude(f => f.Conditions)
            .Where(v => v.FormDefinitionId == formDefinitionId && v.Status == status)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

    private async Task SeedFormAsync(
        SeasbrokerDbContext dbContext,
        string key,
        string name,
        string? description,
        FormSchemaDto schema,
        CancellationToken cancellationToken)
    {
        if (await dbContext.FormDefinitions.AnyAsync(f => f.Key == key, cancellationToken))
        {
            return;
        }

        var definition = new FormDefinition { Key = key, Name = name, Description = description };
        dbContext.FormDefinitions.Add(definition);

        var version = FormMapper.ToNewVersion(definition.Id, 1, FormVersionStatus.Published, schema);
        version.PublishedAt = DateTime.UtcNow;
        dbContext.FormVersions.Add(version);

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded form '{FormKey}' with a published v1 schema.", key);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
