using Seasbroker.Modules.Matching.Application.DTOs;

using Seasbroker.Modules.Matching.Application.Queries;

namespace Seasbroker.Modules.Matching.Application.Services;

public interface IMatchingEngineService
{
    Task<MatchingRunResultDto> RunForCargoAsync(
        Guid cargoListingId,
        CancellationToken cancellationToken = default);

    Task<MatchingRunResultDto> RunForVesselAsync(
        Guid vesselId,
        CancellationToken cancellationToken = default);

    Task<MatchingRunResultDto> RunBatchAsync(CancellationToken cancellationToken = default);
}

public interface IMatchService
{
    Task<MatchRecordDto> CreateManualAsync(
        string cargoListingId,
        string vesselId,
        decimal? score,
        string? matchReason,
        CancellationToken cancellationToken = default);

    Task<MatchRecordDto> ExpireAsync(string matchId, CancellationToken cancellationToken = default);

    Task<MatchRecordDto> CancelAsync(string matchId, CancellationToken cancellationToken = default);
}

public interface IMatchingRuleService
{
    Task<IReadOnlyList<MatchingRuleRecordDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<MatchingRuleRecordDto> UpdateAsync(
        string ruleId,
        decimal? weight,
        bool? isActive,
        string? configuration,
        CancellationToken cancellationToken = default);
}

public interface IMatchQueryService
{
    Task<PocketBaseListResponse<MatchRecordDto>> GetAllAsync(
        GetMatchesQuery query,
        CancellationToken cancellationToken = default);

    Task<MatchRecordDto> GetByIdAsync(string matchId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchRecordDto>> GetForCargoAsync(
        string cargoListingId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchRecordDto>> GetForVesselAsync(
        string vesselId,
        CancellationToken cancellationToken = default);
}
