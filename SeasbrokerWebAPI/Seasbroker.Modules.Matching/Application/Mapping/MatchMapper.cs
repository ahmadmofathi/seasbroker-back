using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Application.Constants;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Engine;

namespace Seasbroker.Modules.Matching.Application.Mapping;

public static class MatchMapper
{
    public static MatchRecordDto ToRecordDto(Match match)
    {
        return new MatchRecordDto
        {
            Id = match.Id.ToString(),
            CollectionId = MatchingConstants.MatchesCollectionName,
            CollectionName = MatchingConstants.MatchesCollectionName,
            Created = match.Created,
            Updated = match.Updated,
            CargoListingId = match.CargoListingId.ToString(),
            VesselId = match.VesselId.ToString(),
            Score = match.Score,
            Status = match.Status,
            Source = match.Source,
            MatchReason = match.MatchReason,
            ScoreBreakdown = match.ScoreBreakdown,
            ExpiresAt = match.ExpiresAt,
            ChatId = match.ChatId?.ToString(),
        };
    }

    public static MatchingRuleRecordDto ToRecordDto(MatchingRule rule)
    {
        return new MatchingRuleRecordDto
        {
            Id = rule.Id.ToString(),
            CollectionId = MatchingConstants.MatchingRulesCollectionName,
            CollectionName = MatchingConstants.MatchingRulesCollectionName,
            Created = rule.Created,
            Updated = rule.Updated,
            Name = rule.Name,
            Criterion = rule.Criterion,
            Weight = rule.Weight,
            IsActive = rule.IsActive,
            Configuration = rule.Configuration,
        };
    }
}
