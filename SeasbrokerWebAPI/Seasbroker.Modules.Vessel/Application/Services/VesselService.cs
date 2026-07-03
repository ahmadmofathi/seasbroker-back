using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Queries;

namespace Seasbroker.Modules.Vessel.Application.Services;

public class VesselService : IVesselService
{
    private readonly IQueryHandler<GetVesselsQuery, PocketBaseListResponse<VesselRecordDto>> _getVesselsHandler;
    private readonly IQueryHandler<GetVesselByIdQuery, VesselRecordDto> _getVesselByIdHandler;
    private readonly ICommandHandler<CreateVesselCommand, VesselRecordDto> _createHandler;
    private readonly ICommandHandler<UpdateVesselCommand, VesselRecordDto> _updateHandler;
    private readonly ICommandHandler<DeactivateVesselCommand> _deactivateHandler;

    public VesselService(
        IQueryHandler<GetVesselsQuery, PocketBaseListResponse<VesselRecordDto>> getVesselsHandler,
        IQueryHandler<GetVesselByIdQuery, VesselRecordDto> getVesselByIdHandler,
        ICommandHandler<CreateVesselCommand, VesselRecordDto> createHandler,
        ICommandHandler<UpdateVesselCommand, VesselRecordDto> updateHandler,
        ICommandHandler<DeactivateVesselCommand> deactivateHandler)
    {
        _getVesselsHandler = getVesselsHandler;
        _getVesselByIdHandler = getVesselByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deactivateHandler = deactivateHandler;
    }

    public Task<PocketBaseListResponse<VesselRecordDto>> GetAllAsync(
        GetVesselsQuery query,
        CancellationToken cancellationToken = default) =>
        _getVesselsHandler.HandleAsync(query, cancellationToken);

    public Task<VesselRecordDto> GetByIdAsync(
        GetVesselByIdQuery query,
        CancellationToken cancellationToken = default) =>
        _getVesselByIdHandler.HandleAsync(query, cancellationToken);

    public Task<VesselRecordDto> CreateAsync(
        CreateVesselCommand command,
        CancellationToken cancellationToken = default) =>
        _createHandler.HandleAsync(command, cancellationToken);

    public Task<VesselRecordDto> UpdateAsync(
        UpdateVesselCommand command,
        CancellationToken cancellationToken = default) =>
        _updateHandler.HandleAsync(command, cancellationToken);

    public Task DeactivateAsync(
        DeactivateVesselCommand command,
        CancellationToken cancellationToken = default) =>
        _deactivateHandler.HandleAsync(command, cancellationToken);
}
