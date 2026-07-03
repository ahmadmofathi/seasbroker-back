using FluentValidation;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Cargo.Application.Abstractions;
using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Helpers;
using Seasbroker.Modules.Cargo.Application.Mapping;
using Seasbroker.Modules.Cargo.Application.Validators;

namespace Seasbroker.Modules.Cargo.Application.Handlers.Commands;

public class UpdateCargoListingCommandHandler : ICommandHandler<UpdateCargoListingCommand, CargoListingRecordDto>
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IValidator<UpdateCargoListingCommand> _validator;

    public UpdateCargoListingCommandHandler(
        SeasbrokerDbContext dbContext,
        IValidator<UpdateCargoListingCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<CargoListingRecordDto> HandleAsync(
        UpdateCargoListingCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);

        var cargoListingId = CargoDomainHelper.ParseCargoListingId(command.CargoListingId);
        var listing = await CargoDomainHelper.GetCargoListingOrThrowAsync(
            _dbContext,
            cargoListingId,
            cancellationToken);

        CargoDomainHelper.EnsureOpenForUpdate(listing);

        if (command.CargoType is not null)
        {
            listing.CargoType = command.CargoType.Trim();
        }

        if (command.Weight.HasValue)
        {
            listing.Weight = command.Weight.Value;
        }

        if (command.Dimensions is not null)
        {
            listing.Dimensions = command.Dimensions.Trim();
        }

        if (command.DeparturePort is not null)
        {
            listing.DeparturePort = command.DeparturePort.Trim();
        }

        if (command.ArrivalPort is not null)
        {
            listing.ArrivalPort = command.ArrivalPort.Trim();
        }

        if (command.DepartureTime.HasValue)
        {
            listing.DepartureTime = command.DepartureTime.Value;
        }

        if (command.ArrivalTime.HasValue)
        {
            listing.ArrivalTime = command.ArrivalTime.Value;
        }

        if (command.AdditionalInfo is not null)
        {
            listing.AdditionalInfo = command.AdditionalInfo.Trim();
        }

        if (command.Priority.HasValue)
        {
            listing.Priority = command.Priority.Value;
        }

        CargoDomainHelper.ValidateDateRange(listing.DepartureTime, listing.ArrivalTime);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CargoMapper.ToRecordDto(listing);
    }
}
