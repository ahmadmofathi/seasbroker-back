using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Mapping;
using Seasbroker.Modules.Vessel.Application.Queries;

namespace Seasbroker.Modules.Vessel.Application.Handlers.Queries;

public class GetVesselsQueryHandler : IQueryHandler<GetVesselsQuery, PocketBaseListResponse<VesselRecordDto>>
{
    private readonly SeasbrokerDbContext _dbContext;

    public GetVesselsQueryHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PocketBaseListResponse<VesselRecordDto>> HandleAsync(
        GetVesselsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var perPage = query.PerPage < 1 ? 50 : Math.Min(query.PerPage, 200);

        var vesselsQuery = _dbContext.Vessels.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            vesselsQuery = vesselsQuery.Where(v => v.Status == query.Status);
        }

        var totalItems = await vesselsQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)perPage);

        var vessels = await vesselsQuery
            .OrderByDescending(v => v.Created)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        return new PocketBaseListResponse<VesselRecordDto>
        {
            Page = page,
            PerPage = perPage,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = vessels.Select(VesselMapper.ToRecordDto).ToList(),
        };
    }
}
