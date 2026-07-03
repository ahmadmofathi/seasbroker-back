using Microsoft.AspNetCore.Http;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.Abstractions;
using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Exceptions;
using Seasbroker.Modules.Cargo.Application.Helpers;
using Seasbroker.Modules.Cargo.Application.Mapping;

namespace Seasbroker.Modules.Cargo.Application.Handlers.Commands;

public class CancelCargoListingCommandHandler : ICommandHandler<CancelCargoListingCommand, CargoListingRecordDto>
{
    private readonly SeasbrokerDbContext _dbContext;

    public CancelCargoListingCommandHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CargoListingRecordDto> HandleAsync(
        CancelCargoListingCommand command,
        CancellationToken cancellationToken = default)
    {
        var cargoListingId = CargoDomainHelper.ParseCargoListingId(command.CargoListingId);
        var listing = await CargoDomainHelper.GetCargoListingOrThrowAsync(
            _dbContext,
            cargoListingId,
            cancellationToken);

        if (listing.Status is CargoStatus.Closed or CargoStatus.Cancelled)
        {
            throw new CargoException(
                "Cargo listing is already in a terminal state.",
                StatusCodes.Status400BadRequest);
        }

        listing.Status = CargoStatus.Cancelled;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CargoMapper.ToRecordDto(listing);
    }
}
