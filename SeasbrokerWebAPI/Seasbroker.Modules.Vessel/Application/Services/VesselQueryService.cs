using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Queries;

namespace Seasbroker.Modules.Vessel.Application.Services;

public class VesselQueryService : IVesselQueryService
{
    private readonly IQueryHandler<GetAvailableVesselsQuery, IReadOnlyList<VesselRecordDto>> _getAvailableHandler;

    public VesselQueryService(
        IQueryHandler<GetAvailableVesselsQuery, IReadOnlyList<VesselRecordDto>> getAvailableHandler)
    {
        _getAvailableHandler = getAvailableHandler;
    }

    public Task<IReadOnlyList<VesselRecordDto>> GetAvailableAsync(
        GetAvailableVesselsQuery query,
        CancellationToken cancellationToken = default) =>
        _getAvailableHandler.HandleAsync(query, cancellationToken);
}
