using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Mapping;

namespace Seasbroker.Modules.Forms.Infrastructure;

/// <summary>
/// Seeds the 3 fixed forms with a published v1 schema reproducing the existing hard-coded
/// forms, the first time the app starts against a database that doesn't have them yet. Runs
/// once per missing form; never touches a form that's already been seeded (and possibly
/// since edited/published by an admin).
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
        }
    }

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
