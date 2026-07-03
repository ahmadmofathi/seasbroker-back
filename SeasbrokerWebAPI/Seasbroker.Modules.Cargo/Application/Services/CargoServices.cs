using Seasbroker.Modules.Cargo.Application.Abstractions;
using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Queries;

namespace Seasbroker.Modules.Cargo.Application.Services;

public class CargoListingService : ICargoListingService
{
    private readonly IQueryHandler<GetCargoListingsQuery, PocketBaseListResponse<CargoListingRecordDto>> _getAllHandler;
    private readonly IQueryHandler<GetCargoListingByIdQuery, CargoListingRecordDto> _getByIdHandler;
    private readonly ICommandHandler<CreateCargoListingCommand, CargoListingRecordDto> _createHandler;
    private readonly ICommandHandler<UpdateCargoListingCommand, CargoListingRecordDto> _updateHandler;
    private readonly ICommandHandler<CloseCargoListingCommand, CargoListingRecordDto> _closeHandler;
    private readonly ICommandHandler<CancelCargoListingCommand, CargoListingRecordDto> _cancelHandler;

    public CargoListingService(
        IQueryHandler<GetCargoListingsQuery, PocketBaseListResponse<CargoListingRecordDto>> getAllHandler,
        IQueryHandler<GetCargoListingByIdQuery, CargoListingRecordDto> getByIdHandler,
        ICommandHandler<CreateCargoListingCommand, CargoListingRecordDto> createHandler,
        ICommandHandler<UpdateCargoListingCommand, CargoListingRecordDto> updateHandler,
        ICommandHandler<CloseCargoListingCommand, CargoListingRecordDto> closeHandler,
        ICommandHandler<CancelCargoListingCommand, CargoListingRecordDto> cancelHandler)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _closeHandler = closeHandler;
        _cancelHandler = cancelHandler;
    }

    public Task<PocketBaseListResponse<CargoListingRecordDto>> GetAllAsync(
        GetCargoListingsQuery query,
        CancellationToken cancellationToken = default) =>
        _getAllHandler.HandleAsync(query, cancellationToken);

    public Task<CargoListingRecordDto> GetByIdAsync(
        GetCargoListingByIdQuery query,
        CancellationToken cancellationToken = default) =>
        _getByIdHandler.HandleAsync(query, cancellationToken);

    public Task<CargoListingRecordDto> CreateAsync(
        CreateCargoListingCommand command,
        CancellationToken cancellationToken = default) =>
        _createHandler.HandleAsync(command, cancellationToken);

    public Task<CargoListingRecordDto> UpdateAsync(
        UpdateCargoListingCommand command,
        CancellationToken cancellationToken = default) =>
        _updateHandler.HandleAsync(command, cancellationToken);

    public Task<CargoListingRecordDto> CloseAsync(
        CloseCargoListingCommand command,
        CancellationToken cancellationToken = default) =>
        _closeHandler.HandleAsync(command, cancellationToken);

    public Task<CargoListingRecordDto> CancelAsync(
        CancelCargoListingCommand command,
        CancellationToken cancellationToken = default) =>
        _cancelHandler.HandleAsync(command, cancellationToken);
}

public class QuotePromotionService : IQuotePromotionService
{
    private readonly ICommandHandler<PromoteQuoteToCargoCommand, CargoListingRecordDto> _promoteHandler;

    public QuotePromotionService(ICommandHandler<PromoteQuoteToCargoCommand, CargoListingRecordDto> promoteHandler)
    {
        _promoteHandler = promoteHandler;
    }

    public Task<CargoListingRecordDto> PromoteAsync(
        PromoteQuoteToCargoCommand command,
        CancellationToken cancellationToken = default) =>
        _promoteHandler.HandleAsync(command, cancellationToken);
}

public class CargoQueryService : ICargoQueryService
{
    private readonly IQueryHandler<GetOpenCargoForMatchingQuery, IReadOnlyList<CargoListingRecordDto>> _openHandler;
    private readonly IQueryHandler<GetCargoByQuoteIdQuery, CargoListingRecordDto?> _byQuoteHandler;

    public CargoQueryService(
        IQueryHandler<GetOpenCargoForMatchingQuery, IReadOnlyList<CargoListingRecordDto>> openHandler,
        IQueryHandler<GetCargoByQuoteIdQuery, CargoListingRecordDto?> byQuoteHandler)
    {
        _openHandler = openHandler;
        _byQuoteHandler = byQuoteHandler;
    }

    public Task<IReadOnlyList<CargoListingRecordDto>> GetOpenForMatchingAsync(
        GetOpenCargoForMatchingQuery query,
        CancellationToken cancellationToken = default) =>
        _openHandler.HandleAsync(query, cancellationToken);

    public Task<CargoListingRecordDto?> GetByQuoteIdAsync(
        GetCargoByQuoteIdQuery query,
        CancellationToken cancellationToken = default) =>
        _byQuoteHandler.HandleAsync(query, cancellationToken);
}
