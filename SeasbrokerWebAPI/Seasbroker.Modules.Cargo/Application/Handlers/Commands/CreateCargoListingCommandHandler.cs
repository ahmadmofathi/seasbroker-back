using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.Abstractions;
using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Helpers;
using Seasbroker.Modules.Cargo.Application.Mapping;
using Seasbroker.Modules.Cargo.Application.Validators;

namespace Seasbroker.Modules.Cargo.Application.Handlers.Commands;

public class CreateCargoListingCommandHandler : ICommandHandler<CreateCargoListingCommand, CargoListingRecordDto>
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IValidator<CreateCargoListingCommand> _validator;

    public CreateCargoListingCommandHandler(
        SeasbrokerDbContext dbContext,
        IValidator<CreateCargoListingCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<CargoListingRecordDto> HandleAsync(
        CreateCargoListingCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);

        var customerId = CargoDomainHelper.ParseCustomerId(command.CustomerId);
        await CargoDomainHelper.EnsureCustomerExistsAsync(_dbContext, customerId, cancellationToken);

        var requestedQuoteId = CargoDomainHelper.ParseOptionalQuoteId(command.RequestedQuoteId);

        if (requestedQuoteId.HasValue)
        {
            await CargoDomainHelper.EnsureQuoteExistsAsync(_dbContext, requestedQuoteId.Value, cancellationToken);
            await CargoDomainHelper.EnsureQuoteNotPromotedAsync(_dbContext, requestedQuoteId.Value, cancellationToken);
        }

        CargoDomainHelper.ValidateDateRange(command.DepartureTime, command.ArrivalTime);

        var referenceNumber = string.IsNullOrWhiteSpace(command.ReferenceNumber)
            ? await CargoDomainHelper.GenerateReferenceNumberAsync(_dbContext, cancellationToken)
            : command.ReferenceNumber.Trim();

        await CargoDomainHelper.EnsureReferenceNumberUniqueAsync(_dbContext, referenceNumber, null, cancellationToken);

        var listing = new CargoListing
        {
            CustomerId = customerId,
            RequestedQuoteId = requestedQuoteId,
            ReferenceNumber = referenceNumber,
            CargoType = command.CargoType.Trim(),
            Weight = command.Weight,
            Dimensions = command.Dimensions.Trim(),
            DeparturePort = command.DeparturePort.Trim(),
            DepartureTime = command.DepartureTime,
            ArrivalPort = command.ArrivalPort.Trim(),
            ArrivalTime = command.ArrivalTime,
            AdditionalInfo = command.AdditionalInfo?.Trim(),
            Status = CargoDomainHelper.ResolveStatus(command.Status),
            Priority = CargoDomainHelper.ResolvePriority(command.Priority),
        };

        _dbContext.CargoListings.Add(listing);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CargoMapper.ToRecordDto(listing);
    }
}
