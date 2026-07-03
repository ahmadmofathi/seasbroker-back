using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Queries;

namespace Seasbroker.Modules.Vessel.Application.Services;

public interface IVesselAvailabilityService
{
    Task<IReadOnlyList<VesselAvailabilityRecordDto>> GetByVesselIdAsync(
        GetVesselAvailabilitiesQuery query,
        CancellationToken cancellationToken = default);

    Task<VesselAvailabilityRecordDto> CreateAsync(
        CreateVesselAvailabilityCommand command,
        CancellationToken cancellationToken = default);

    Task<VesselAvailabilityRecordDto> UpdateAsync(
        UpdateVesselAvailabilityCommand command,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        DeactivateVesselAvailabilityCommand command,
        CancellationToken cancellationToken = default);
}
