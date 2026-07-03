using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Application.Constants;

namespace Seasbroker.Modules.Matching.Infrastructure;

public class MatchingRuleSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MatchingRuleSeeder> _logger;

    public MatchingRuleSeeder(
        IServiceScopeFactory scopeFactory,
        ILogger<MatchingRuleSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SeasbrokerDbContext>();

        if (await dbContext.MatchingRules.AnyAsync(cancellationToken))
        {
            return;
        }

        var rules = new[]
        {
            CreateRule("Port Compatibility", MatchingConstants.CriterionPort, 30m),
            CreateRule("Date Overlap", MatchingConstants.CriterionDate, 25m),
            CreateRule("Capacity Compatibility", MatchingConstants.CriterionCapacity, 25m),
            CreateRule("Cargo/Vessel Type", MatchingConstants.CriterionType, 15m),
            CreateRule("Priority Boost", MatchingConstants.CriterionPriority, 5m),
        };

        dbContext.MatchingRules.AddRange(rules);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} default matching rules.", rules.Length);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static MatchingRule CreateRule(string name, string criterion, decimal weight) =>
        new()
        {
            Name = name,
            Criterion = criterion,
            Weight = weight,
            IsActive = true,
        };
}
