using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.Abstractions;
using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Exceptions;
using Seasbroker.Modules.Cargo.Application.Helpers;
using Seasbroker.Modules.Cargo.Application.Mapping;
using Seasbroker.Modules.Cargo.Application.Validators;

namespace Seasbroker.Modules.Cargo.Application.Handlers.Commands;

public class PromoteQuoteToCargoCommandHandler : ICommandHandler<PromoteQuoteToCargoCommand, CargoListingRecordDto>
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IValidator<PromoteQuoteToCargoCommand> _validator;

    public PromoteQuoteToCargoCommandHandler(
        SeasbrokerDbContext dbContext,
        IValidator<PromoteQuoteToCargoCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<CargoListingRecordDto> HandleAsync(
        PromoteQuoteToCargoCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);

        if (!Guid.TryParse(command.RequestedQuoteId, out var quoteId))
        {
            throw new CargoException("The requested quote wasn't found.", StatusCodes.Status404NotFound);
        }

        await CargoDomainHelper.EnsureQuoteNotPromotedAsync(_dbContext, quoteId, cancellationToken);

        var quote = await _dbContext.RequestedQuotes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);

        if (quote is null)
        {
            throw new CargoException("The requested quote wasn't found.", StatusCodes.Status404NotFound);
        }

        var departureTime = QuoteDateParser.ParseOrThrow(quote.DepartureTime, "departureTime");
        var arrivalTime = QuoteDateParser.ParseOrThrow(quote.ArrivalTime, "arrivalTime");
        CargoDomainHelper.ValidateDateRange(departureTime, arrivalTime);

        var referenceNumber = string.IsNullOrWhiteSpace(command.ReferenceNumber)
            ? await CargoDomainHelper.GenerateReferenceNumberAsync(_dbContext, cancellationToken)
            : command.ReferenceNumber.Trim();

        await CargoDomainHelper.EnsureReferenceNumberUniqueAsync(_dbContext, referenceNumber, null, cancellationToken);

        var listing = new CargoListing
        {
            CustomerId = quote.CustomerId,
            RequestedQuoteId = quote.Id,
            ReferenceNumber = referenceNumber,
            CargoType = quote.CargoType,
            Weight = quote.Weight,
            Dimensions = quote.Dimensions,
            DeparturePort = quote.DeparturePort,
            DepartureTime = departureTime,
            ArrivalPort = quote.ArrivalPort,
            ArrivalTime = arrivalTime,
            AdditionalInfo = quote.AdditionalInfo,
            Status = CargoDomainHelper.ResolveStatus(command.Status),
            Priority = CargoDomainHelper.ResolvePriority(command.Priority),
        };

        _dbContext.CargoListings.Add(listing);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new CargoException(
                "This quote has already been promoted to a cargo listing.",
                StatusCodes.Status409Conflict);
        }

        return CargoMapper.ToRecordDto(listing);
    }
}
