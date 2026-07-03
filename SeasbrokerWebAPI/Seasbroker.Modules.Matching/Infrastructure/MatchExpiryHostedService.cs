using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Infrastructure.Options;

namespace Seasbroker.Modules.Matching.Infrastructure;

public class MatchExpiryHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MatchingOptions _options;
    private readonly ILogger<MatchExpiryHostedService> _logger;

    public MatchExpiryHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<MatchingOptions> options,
        ILogger<MatchExpiryHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireProposedMatchesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to expire proposed matches.");
            }

            await Task.Delay(TimeSpan.FromMinutes(_options.ExpiryWorkerIntervalMinutes), stoppingToken);
        }
    }

    public async Task<int> ExpireProposedMatchesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SeasbrokerDbContext>();
        var utcNow = DateTime.UtcNow;

        var expiredMatches = await dbContext.Matches
            .Where(m => (m.Status == MatchStatus.Proposed || m.Status == MatchStatus.PendingApproval) &&
                        m.ExpiresAt != null &&
                        m.ExpiresAt < utcNow)
            .ToListAsync(cancellationToken);

        if (expiredMatches.Count == 0)
        {
            return 0;
        }

        foreach (var match in expiredMatches)
        {
            match.Status = MatchStatus.Expired;
            match.ExpiresAt = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expired {Count} proposed or pending approval match(es).", expiredMatches.Count);
        return expiredMatches.Count;
    }
}
