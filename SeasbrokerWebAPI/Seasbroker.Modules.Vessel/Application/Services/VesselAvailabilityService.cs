using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Queries;

namespace Seasbroker.Modules.Vessel.Application.Services;

public class VesselAvailabilityService : IVesselAvailabilityService
{
    private readonly IQueryHandler<GetVesselAvailabilitiesQuery, IReadOnlyList<VesselAvailabilityRecordDto>> _getHandler;
    private readonly ICommandHandler<CreateVesselAvailabilityCommand, VesselAvailabilityRecordDto> _createHandler;
    private readonly ICommandHandler<UpdateVesselAvailabilityCommand, VesselAvailabilityRecordDto> _updateHandler;
    private readonly ICommandHandler<DeactivateVesselAvailabilityCommand> _deactivateHandler;

    public VesselAvailabilityService(
        IQueryHandler<GetVesselAvailabilitiesQuery, IReadOnlyList<VesselAvailabilityRecordDto>> getHandler,
        ICommandHandler<CreateVesselAvailabilityCommand, VesselAvailabilityRecordDto> createHandler,
        ICommandHandler<UpdateVesselAvailabilityCommand, VesselAvailabilityRecordDto> updateHandler,
        ICommandHandler<DeactivateVesselAvailabilityCommand> deactivateHandler)
    {
        _getHandler = getHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deactivateHandler = deactivateHandler;
    }

    public Task<IReadOnlyList<VesselAvailabilityRecordDto>> GetByVesselIdAsync(
        GetVesselAvailabilitiesQuery query,
        CancellationToken cancellationToken = default) =>
        _getHandler.HandleAsync(query, cancellationToken);

    public Task<VesselAvailabilityRecordDto> CreateAsync(
        CreateVesselAvailabilityCommand command,
        CancellationToken cancellationToken = default) =>
        _createHandler.HandleAsync(command, cancellationToken);

    public Task<VesselAvailabilityRecordDto> UpdateAsync(
        UpdateVesselAvailabilityCommand command,
        CancellationToken cancellationToken = default) =>
        _updateHandler.HandleAsync(command, cancellationToken);

    public Task DeactivateAsync(
        DeactivateVesselAvailabilityCommand command,
        CancellationToken cancellationToken = default) =>
        _deactivateHandler.HandleAsync(command, cancellationToken);
}
