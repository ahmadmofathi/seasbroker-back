using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Helpers;
using Seasbroker.Modules.Matching.Application.Mapping;

namespace Seasbroker.Modules.Matching.Application.Services;

public class MatchingRuleService : IMatchingRuleService
{
    private readonly SeasbrokerDbContext _dbContext;

    public MatchingRuleService(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MatchingRuleRecordDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _dbContext.MatchingRules
            .AsNoTracking()
            .OrderBy(r => r.Criterion)
            .ToListAsync(cancellationToken);

        return rules.Select(MatchMapper.ToRecordDto).ToList();
    }

    public async Task<MatchingRuleRecordDto> UpdateAsync(
        string ruleId,
        decimal? weight,
        bool? isActive,
        string? configuration,
        CancellationToken cancellationToken = default)
    {
        var parsedRuleId = MatchingDomainHelper.ParseGuidOrNotFound(ruleId, "matching rule");
        var rule = await MatchingDomainHelper.GetMatchingRuleOrThrowAsync(_dbContext, parsedRuleId, cancellationToken);

        if (weight.HasValue)
        {
            rule.Weight = weight.Value;
        }

        if (isActive.HasValue)
        {
            rule.IsActive = isActive.Value;
        }

        if (configuration is not null)
        {
            rule.Configuration = configuration;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MatchMapper.ToRecordDto(rule);
    }
}
