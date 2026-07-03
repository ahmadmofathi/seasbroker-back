using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Queries;

namespace Seasbroker.Modules.Vessel.Application.Services;

public interface IVesselService
{
    Task<PocketBaseListResponse<VesselRecordDto>> GetAllAsync(
        GetVesselsQuery query,
        CancellationToken cancellationToken = default);

    Task<VesselRecordDto> GetByIdAsync(
        GetVesselByIdQuery query,
        CancellationToken cancellationToken = default);

    Task<VesselRecordDto> CreateAsync(
        CreateVesselCommand command,
        CancellationToken cancellationToken = default);

    Task<VesselRecordDto> UpdateAsync(
        UpdateVesselCommand command,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        DeactivateVesselCommand command,
        CancellationToken cancellationToken = default);
}
