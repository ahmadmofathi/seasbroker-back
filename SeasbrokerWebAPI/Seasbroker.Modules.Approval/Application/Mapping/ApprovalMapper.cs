using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Approval.Application.Constants;
using Seasbroker.Modules.Approval.Application.DTOs;

namespace Seasbroker.Modules.Approval.Application.Mapping;

public static class ApprovalMapper
{
    public static MatchApprovalRecordDto ToRecordDto(Match match)
    {
        return new MatchApprovalRecordDto
        {
            Id = match.Id.ToString(),
            CollectionId = ApprovalConstants.MatchesCollectionName,
            CollectionName = ApprovalConstants.MatchesCollectionName,
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
            ApprovedBy = match.ApprovedBy?.ToString(),
            ApprovedAt = match.ApprovedAt,
            RejectedBy = match.RejectedBy?.ToString(),
            RejectedAt = match.RejectedAt,
            CancelledBy = match.CancelledBy?.ToString(),
            CancelledAt = match.CancelledAt,
            CompletedBy = match.CompletedBy?.ToString(),
            CompletedAt = match.CompletedAt,
            Reason = match.Reason,
            RowVersion = match.RowVersion.Length == 0
                ? string.Empty
                : Convert.ToBase64String(match.RowVersion),
        };
    }
}
