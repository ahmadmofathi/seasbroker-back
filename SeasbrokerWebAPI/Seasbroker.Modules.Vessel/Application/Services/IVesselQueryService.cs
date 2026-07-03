using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Queries;

namespace Seasbroker.Modules.Vessel.Application.Services;

public interface IVesselQueryService
{
    Task<IReadOnlyList<VesselRecordDto>> GetAvailableAsync(
        GetAvailableVesselsQuery query,
        CancellationToken cancellationToken = default);
}
