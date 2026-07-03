using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Exceptions;
using Seasbroker.Modules.Vessel.Application.Helpers;
using Seasbroker.Modules.Vessel.Application.Mapping;
using Seasbroker.Modules.Vessel.Application.Queries;

namespace Seasbroker.Modules.Vessel.Application.Handlers.Queries;

public class GetVesselAvailabilitiesQueryHandler
    : IQueryHandler<GetVesselAvailabilitiesQuery, IReadOnlyList<VesselAvailabilityRecordDto>>
{
    private readonly SeasbrokerDbContext _dbContext;

    public GetVesselAvailabilitiesQueryHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VesselAvailabilityRecordDto>> HandleAsync(
        GetVesselAvailabilitiesQuery query,
        CancellationToken cancellationToken = default)
    {
        var vesselId = VesselDomainHelper.ParseVesselId(query.VesselId);

        var vesselExists = await _dbContext.Vessels
            .AsNoTracking()
            .AnyAsync(v => v.Id == vesselId, cancellationToken);

        if (!vesselExists)
        {
            throw new VesselException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        var availabilitiesQuery = _dbContext.VesselAvailabilities
            .AsNoTracking()
            .Where(a => a.VesselId == vesselId);

        if (query.ActiveOnly)
        {
            availabilitiesQuery = availabilitiesQuery.Where(a => a.IsActive);
        }

        var availabilities = await availabilitiesQuery
            .OrderBy(a => a.AvailableFrom)
            .ToListAsync(cancellationToken);

        return availabilities.Select(VesselMapper.ToRecordDto).ToList();
    }
}
