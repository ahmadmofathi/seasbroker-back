using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Mapping;
using Seasbroker.Modules.Vessel.Application.Queries;

namespace Seasbroker.Modules.Vessel.Application.Handlers.Queries;

public class GetAvailableVesselsQueryHandler : IQueryHandler<GetAvailableVesselsQuery, IReadOnlyList<VesselRecordDto>>
{
    private readonly SeasbrokerDbContext _dbContext;

    public GetAvailableVesselsQueryHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VesselRecordDto>> HandleAsync(
        GetAvailableVesselsQuery query,
        CancellationToken cancellationToken = default)
    {
        var vesselsQuery = _dbContext.Vessels
            .AsNoTracking()
            .Where(v => v.Status == VesselStatus.Active);

        if (!string.IsNullOrWhiteSpace(query.OpenPort))
        {
            var openPort = query.OpenPort.Trim();

            vesselsQuery = vesselsQuery.Where(v =>
                v.CurrentPort == openPort ||
                v.Availabilities.Any(a =>
                    a.IsActive &&
                    a.OpenPort == openPort &&
                    (!query.AvailableFrom.HasValue || a.AvailableTo > query.AvailableFrom) &&
                    (!query.AvailableTo.HasValue || a.AvailableFrom < query.AvailableTo)));
        }
        else if (query.AvailableFrom.HasValue || query.AvailableTo.HasValue)
        {
            vesselsQuery = vesselsQuery.Where(v =>
                v.Availabilities.Any(a =>
                    a.IsActive &&
                    (!query.AvailableFrom.HasValue || a.AvailableTo > query.AvailableFrom) &&
                    (!query.AvailableTo.HasValue || a.AvailableFrom < query.AvailableTo)));
        }
        else
        {
            vesselsQuery = vesselsQuery.Where(v => v.Availabilities.Any(a => a.IsActive));
        }

        var vessels = await vesselsQuery
            .OrderByDescending(v => v.Created)
            .ToListAsync(cancellationToken);

        return vessels.Select(VesselMapper.ToRecordDto).ToList();
    }
}
