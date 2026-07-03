using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Quote.Application.Commands;
using Seasbroker.Modules.Quote.Application.DTOs;
using Seasbroker.Modules.Quote.Application.Exceptions;
using Seasbroker.Modules.Quote.Application.Mapping;
using Seasbroker.Modules.Quote.Application.Queries;

namespace Seasbroker.Modules.Quote.Application.Services;

public class QuoteService : IQuoteService
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly ICustomerService _customerService;

    public QuoteService(SeasbrokerDbContext dbContext, ICustomerService customerService)
    {
        _dbContext = dbContext;
        _customerService = customerService;
    }

    public async Task<CreateQuoteResponse> CreateAsync(
        CreateQuoteCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingCustomer = await _customerService.GetByEmailAsync(
            new GetCustomerByEmailQuery(command.Email),
            cancellationToken);

        Guid customerId;

        if (existingCustomer is null)
        {
            var createdCustomer = await _customerService.CreateAsync(
                new CreateCustomerCommand(
                    command.Email,
                    command.PhoneNumber,
                    command.Fname,
                    command.Lname),
                cancellationToken);

            if (!Guid.TryParse(createdCustomer.Id, out customerId))
            {
                throw new QuoteException(
                    "Cannot save new customer record",
                    StatusCodes.Status500InternalServerError);
            }
        }
        else
        {
            if (!Guid.TryParse(existingCustomer.Id, out customerId))
            {
                throw new QuoteException("Cannot access customers table", StatusCodes.Status500InternalServerError);
            }
        }

        var quote = new global::Seasbroker.Infrastructure.Persistence.Entities.RequestedQuote
        {
            CustomerId = customerId,
            CargoType = command.CargoType,
            Weight = command.Weight,
            DeparturePort = command.DeparturePort,
            DepartureTime = command.DepartureTime,
            ArrivalPort = command.ArrivalPort,
            ArrivalTime = command.ArrivalTime,
            Dimensions = command.Dimensions,
            AdditionalInfo = command.AdditionalInfo,
        };

        _dbContext.RequestedQuotes.Add(quote);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new QuoteException("Failed to save quote", StatusCodes.Status500InternalServerError, ex.Message);
        }

        return new CreateQuoteResponse();
    }

    public async Task<IReadOnlyList<RequestedQuoteRecordDto>> GetAllAsync(
        GetRequestedQuotesQuery query,
        CancellationToken cancellationToken = default)
    {
        var quotes = await _dbContext.RequestedQuotes
            .AsNoTracking()
            .OrderByDescending(q => q.Created)
            .ToListAsync(cancellationToken);

        return quotes.Select(QuoteMapper.ToRecordDto).ToList();
    }
}
