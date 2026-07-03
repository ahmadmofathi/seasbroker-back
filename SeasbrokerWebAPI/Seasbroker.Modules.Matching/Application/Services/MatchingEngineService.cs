using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Application.Abstractions;
using Seasbroker.Modules.Matching.Application.Constants;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Engine;
using Seasbroker.Modules.Matching.Application.Events;
using Seasbroker.Modules.Matching.Application.Helpers;
using Seasbroker.Modules.Matching.Application.Mapping;
using Seasbroker.Modules.Matching.Infrastructure.Options;

namespace Seasbroker.Modules.Matching.Application.Services;

public class MatchingEngineService : IMatchingEngineService
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly MatchingOptions _options;

    public MatchingEngineService(
        SeasbrokerDbContext dbContext,
        IDomainEventDispatcher eventDispatcher,
        IOptions<MatchingOptions> options)
    {
        _dbContext = dbContext;
        _eventDispatcher = eventDispatcher;
        _options = options.Value;
    }

    public async Task<MatchingRunResultDto> RunForCargoAsync(
        Guid cargoListingId,
        CancellationToken cancellationToken = default)
    {
        var cargo = await MatchingDomainHelper.GetOpenCargoOrThrowAsync(_dbContext, cargoListingId, cancellationToken);
        var ruleWeights = await GetActiveRuleWeightsAsync(cancellationToken);
        var eligibleVessels = await GetEligibleVesselsForCargoAsync(cargo, cancellationToken);

        return await CreateMatchesForCargoAsync(cargo, eligibleVessels, ruleWeights, cancellationToken);
    }

    public async Task<MatchingRunResultDto> RunForVesselAsync(
        Guid vesselId,
        CancellationToken cancellationToken = default)
    {
        var vessel = await MatchingDomainHelper.GetActiveVesselOrThrowAsync(_dbContext, vesselId, cancellationToken);
        var ruleWeights = await GetActiveRuleWeightsAsync(cancellationToken);
        var openCargo = await _dbContext.CargoListings
            .AsNoTracking()
            .Where(c => c.Status == CargoStatus.Open)
            .ToListAsync(cancellationToken);

        var aggregate = new MatchingRunResultDto();
        var allItems = new List<MatchRecordDto>();

        foreach (var cargo in openCargo)
        {
            var eligible = await GetEligibleVesselsForCargoAsync(cargo, cancellationToken);
            if (!eligible.Any(e => e.Vessel.Id == vessel.Id))
            {
                continue;
            }

            var result = await CreateMatchesForCargoAsync(
                cargo,
                eligible.Where(e => e.Vessel.Id == vessel.Id).ToList(),
                ruleWeights,
                cancellationToken);

            aggregate.MatchesCreated += result.MatchesCreated;
            aggregate.MatchesSkipped += result.MatchesSkipped;
            allItems.AddRange(result.Items);
        }

        aggregate.Items = allItems;
        return aggregate;
    }

    public async Task<MatchingRunResultDto> RunBatchAsync(CancellationToken cancellationToken = default)
    {
        var openCargo = await _dbContext.CargoListings
            .AsNoTracking()
            .Where(c => c.Status == CargoStatus.Open)
            .ToListAsync(cancellationToken);

        var ruleWeights = await GetActiveRuleWeightsAsync(cancellationToken);
        var aggregate = new MatchingRunResultDto();
        var allItems = new List<MatchRecordDto>();

        foreach (var cargo in openCargo)
        {
            var eligibleVessels = await GetEligibleVesselsForCargoAsync(cargo, cancellationToken);
            var result = await CreateMatchesForCargoAsync(cargo, eligibleVessels, ruleWeights, cancellationToken);

            aggregate.MatchesCreated += result.MatchesCreated;
            aggregate.MatchesSkipped += result.MatchesSkipped;
            allItems.AddRange(result.Items);
        }

        aggregate.Items = allItems;
        return aggregate;
    }

    private async Task<MatchingRunResultDto> CreateMatchesForCargoAsync(
        CargoListing cargo,
        IReadOnlyList<EligibleVesselCandidate> eligibleVessels,
        IReadOnlyDictionary<string, decimal> ruleWeights,
        CancellationToken cancellationToken)
    {
        var scoredCandidates = new List<(EligibleVesselCandidate Candidate, MatchScoreResult Score)>();

        foreach (var candidate in eligibleVessels)
        {
            if (await HasActiveDuplicateAsync(cargo.Id, candidate.Vessel.Id, cancellationToken))
            {
                continue;
            }

            var score = MatchingScoreCalculator.Calculate(
                cargo,
                candidate.Vessel,
                candidate.Availability,
                ruleWeights);

            if (score.TotalScore >= _options.MinScore)
            {
                scoredCandidates.Add((candidate, score));
            }
        }

        var topCandidates = scoredCandidates
            .OrderByDescending(x => x.Score.TotalScore)
            .Take(_options.MaxProposalsPerCargo)
            .ToList();

        var created = new List<MatchRecordDto>();
        var skipped = scoredCandidates.Count - topCandidates.Count;
        if (scoredCandidates.Count < eligibleVessels.Count)
        {
            skipped += eligibleVessels.Count - scoredCandidates.Count;
        }

        foreach (var (candidate, score) in topCandidates)
        {
            var match = await PersistAutomaticMatchAsync(
                cargo,
                candidate.Vessel,
                score,
                cancellationToken);

            created.Add(MatchMapper.ToRecordDto(match));
        }

        return new MatchingRunResultDto
        {
            MatchesCreated = created.Count,
            MatchesSkipped = Math.Max(0, skipped),
            Items = created,
        };
    }

    private async Task<Match> PersistAutomaticMatchAsync(
        CargoListing cargo,
        Vessel vessel,
        MatchScoreResult score,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var match = new Match
        {
            CargoListingId = cargo.Id,
            VesselId = vessel.Id,
            Score = score.TotalScore,
            Status = MatchStatus.Proposed,
            Source = MatchSource.Automatic,
            MatchReason = score.MatchReason,
            ScoreBreakdown = score.ToBreakdownJson(),
            ExpiresAt = utcNow.AddHours(_options.ProposalTtlHours),
        };

        _dbContext.Matches.Add(match);

        match.Status = MatchStatus.PendingApproval;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventDispatcher.PublishAsync(
            new MatchPendingApprovalEvent(match.Id, match.CargoListingId, match.VesselId, match.Score, match.Source),
            cancellationToken);

        return match;
    }

    private async Task<bool> HasActiveDuplicateAsync(
        Guid cargoListingId,
        Guid vesselId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Matches
            .AsNoTracking()
            .AnyAsync(
                m => m.CargoListingId == cargoListingId &&
                     m.VesselId == vesselId &&
                     MatchStatus.ActivePairStatusFilter.Contains(m.Status),
                cancellationToken);
    }

    private async Task<IReadOnlyList<EligibleVesselCandidate>> GetEligibleVesselsForCargoAsync(
        CargoListing cargo,
        CancellationToken cancellationToken)
    {
        var vessels = await _dbContext.Vessels
            .AsNoTracking()
            .Include(v => v.Availabilities.Where(a => a.IsActive))
            .Where(v => v.Status == VesselStatus.Active && v.Dwt >= cargo.Weight)
            .ToListAsync(cancellationToken);

        var candidates = new List<EligibleVesselCandidate>();

        foreach (var vessel in vessels)
        {
            foreach (var availability in vessel.Availabilities.Where(a => a.IsActive))
            {
                if (availability.AvailableFrom < cargo.ArrivalTime &&
                    availability.AvailableTo > cargo.DepartureTime)
                {
                    candidates.Add(new EligibleVesselCandidate(vessel, availability));
                }
            }
        }

        return candidates;
    }

    private async Task<IReadOnlyDictionary<string, decimal>> GetActiveRuleWeightsAsync(
        CancellationToken cancellationToken)
    {
        var rules = await _dbContext.MatchingRules
            .AsNoTracking()
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);

        if (rules.Count == 0)
        {
            return new Dictionary<string, decimal>
            {
                [MatchingConstants.CriterionPort] = 30m,
                [MatchingConstants.CriterionDate] = 25m,
                [MatchingConstants.CriterionCapacity] = 25m,
                [MatchingConstants.CriterionType] = 15m,
                [MatchingConstants.CriterionPriority] = 5m,
            };
        }

        return rules.ToDictionary(r => r.Criterion, r => r.Weight);
    }

    private sealed record EligibleVesselCandidate(Vessel Vessel, VesselAvailability Availability);
}
