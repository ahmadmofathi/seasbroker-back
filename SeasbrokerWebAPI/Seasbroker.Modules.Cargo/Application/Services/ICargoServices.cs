using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Queries;

namespace Seasbroker.Modules.Cargo.Application.Services;

public interface ICargoListingService
{
    Task<PocketBaseListResponse<CargoListingRecordDto>> GetAllAsync(
        GetCargoListingsQuery query,
        CancellationToken cancellationToken = default);

    Task<CargoListingRecordDto> GetByIdAsync(
        GetCargoListingByIdQuery query,
        CancellationToken cancellationToken = default);

    Task<CargoListingRecordDto> CreateAsync(
        CreateCargoListingCommand command,
        CancellationToken cancellationToken = default);

    Task<CargoListingRecordDto> UpdateAsync(
        UpdateCargoListingCommand command,
        CancellationToken cancellationToken = default);

    Task<CargoListingRecordDto> CloseAsync(
        CloseCargoListingCommand command,
        CancellationToken cancellationToken = default);

    Task<CargoListingRecordDto> CancelAsync(
        CancelCargoListingCommand command,
        CancellationToken cancellationToken = default);
}

public interface IQuotePromotionService
{
    Task<CargoListingRecordDto> PromoteAsync(
        PromoteQuoteToCargoCommand command,
        CancellationToken cancellationToken = default);
}

public interface ICargoQueryService
{
    Task<IReadOnlyList<CargoListingRecordDto>> GetOpenForMatchingAsync(
        GetOpenCargoForMatchingQuery query,
        CancellationToken cancellationToken = default);

    Task<CargoListingRecordDto?> GetByQuoteIdAsync(
        GetCargoByQuoteIdQuery query,
        CancellationToken cancellationToken = default);
}
