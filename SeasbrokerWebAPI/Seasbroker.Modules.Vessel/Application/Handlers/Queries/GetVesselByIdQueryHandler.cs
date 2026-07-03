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

public class GetVesselByIdQueryHandler : IQueryHandler<GetVesselByIdQuery, VesselRecordDto>
{
    private readonly SeasbrokerDbContext _dbContext;

    public GetVesselByIdQueryHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VesselRecordDto> HandleAsync(
        GetVesselByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var vesselId = VesselDomainHelper.ParseVesselId(query.VesselId);

        var vessel = await _dbContext.Vessels
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == vesselId, cancellationToken);

        if (vessel is null)
        {
            throw new VesselException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return VesselMapper.ToRecordDto(vessel);
    }
}
