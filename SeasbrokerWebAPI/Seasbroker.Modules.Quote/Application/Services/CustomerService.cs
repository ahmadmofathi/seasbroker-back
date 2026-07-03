using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Quote.Application.Commands;
using Seasbroker.Modules.Quote.Application.DTOs;
using Seasbroker.Modules.Quote.Application.Exceptions;
using Seasbroker.Modules.Quote.Application.Mapping;
using Seasbroker.Modules.Quote.Application.Queries;

namespace Seasbroker.Modules.Quote.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly SeasbrokerDbContext _dbContext;

    public CustomerService(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerRecordDto?> GetByEmailAsync(
        GetCustomerByEmailQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _dbContext.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == query.Email, cancellationToken);

            return customer is null ? null : QuoteMapper.ToRecordDto(customer);
        }
        catch (Exception ex)
        {
            throw new QuoteException("Cannot access customers table", StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    public async Task<CustomerRecordDto> CreateAsync(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = new global::Seasbroker.Infrastructure.Persistence.Entities.Customer
        {
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            FirstName = command.FirstName,
            LastName = command.LastName,
        };

        _dbContext.Customers.Add(customer);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new QuoteException("Cannot save new customer record", StatusCodes.Status500InternalServerError, ex.Message);
        }

        return QuoteMapper.ToRecordDto(customer);
    }
}
