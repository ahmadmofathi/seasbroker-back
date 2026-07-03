using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Exceptions;
using Seasbroker.Modules.Matching.Application.Helpers;
using Seasbroker.Modules.Matching.Application.Mapping;
using Seasbroker.Modules.Matching.Application.Queries;

namespace Seasbroker.Modules.Matching.Application.Services;

public class MatchQueryService : IMatchQueryService
{
    private readonly SeasbrokerDbContext _dbContext;

    public MatchQueryService(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PocketBaseListResponse<MatchRecordDto>> GetAllAsync(
        GetMatchesQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var perPage = query.PerPage < 1 ? 50 : Math.Min(query.PerPage, 200);

        var matchesQuery = _dbContext.Matches.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            matchesQuery = matchesQuery.Where(m => m.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.CargoListingId) &&
            Guid.TryParse(query.CargoListingId, out var cargoListingId))
        {
            matchesQuery = matchesQuery.Where(m => m.CargoListingId == cargoListingId);
        }

        if (!string.IsNullOrWhiteSpace(query.VesselId) &&
            Guid.TryParse(query.VesselId, out var vesselId))
        {
            matchesQuery = matchesQuery.Where(m => m.VesselId == vesselId);
        }

        var totalItems = await matchesQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)perPage);

        var matches = await matchesQuery
            .OrderByDescending(m => m.Score)
            .ThenByDescending(m => m.Created)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        return new PocketBaseListResponse<MatchRecordDto>
        {
            Page = page,
            PerPage = perPage,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = matches.Select(MatchMapper.ToRecordDto).ToList(),
        };
    }

    public async Task<MatchRecordDto> GetByIdAsync(string matchId, CancellationToken cancellationToken = default)
    {
        var parsedMatchId = MatchingDomainHelper.ParseGuidOrNotFound(matchId, "match");

        var match = await _dbContext.Matches
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == parsedMatchId, cancellationToken);

        if (match is null)
        {
            throw new MatchingException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return MatchMapper.ToRecordDto(match);
    }

    public async Task<IReadOnlyList<MatchRecordDto>> GetForCargoAsync(
        string cargoListingId,
        CancellationToken cancellationToken = default)
    {
        var parsedCargoId = MatchingDomainHelper.ParseGuidOrNotFound(cargoListingId, "cargo listing");

        var matches = await _dbContext.Matches
            .AsNoTracking()
            .Where(m => m.CargoListingId == parsedCargoId)
            .OrderByDescending(m => m.Score)
            .ToListAsync(cancellationToken);

        return matches.Select(MatchMapper.ToRecordDto).ToList();
    }

    public async Task<IReadOnlyList<MatchRecordDto>> GetForVesselAsync(
        string vesselId,
        CancellationToken cancellationToken = default)
    {
        var parsedVesselId = MatchingDomainHelper.ParseGuidOrNotFound(vesselId, "vessel");

        var matches = await _dbContext.Matches
            .AsNoTracking()
            .Where(m => m.VesselId == parsedVesselId)
            .OrderByDescending(m => m.Score)
            .ToListAsync(cancellationToken);

        return matches.Select(MatchMapper.ToRecordDto).ToList();
    }
}
