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
        var departureTime = ParseDateOrThrow(command.DepartureTime, "departureTime");
        var arrivalTime = ParseDateOrThrow(command.ArrivalTime, "arrivalTime");

        if (departureTime >= arrivalTime)
        {
            throw new QuoteException(
                "departureTime must be before arrivalTime.",
                StatusCodes.Status400BadRequest);
        }

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

        return new CreateQuoteResponse
        {
            Id = quote.Id.ToString(),
            RequestedQuoteId = quote.Id.ToString(),
        };
    }

    public async Task<PocketBaseListResponse<RequestedQuoteRecordDto>> GetAllAsync(
        GetRequestedQuotesQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var perPage = query.PerPage < 1 ? 50 : Math.Min(query.PerPage, 200);

        var totalItems = await _dbContext.RequestedQuotes
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var quotes = await _dbContext.RequestedQuotes
            .AsNoTracking()
            .Include(q => q.Customer)
            .OrderByDescending(q => q.Created)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)perPage);

        var quoteIds = quotes.Select(q => q.Id).ToList();
        var promotedListingIdByQuoteId = await _dbContext.CargoListings
            .AsNoTracking()
            .Where(c => c.RequestedQuoteId != null && quoteIds.Contains(c.RequestedQuoteId.Value))
            .ToDictionaryAsync(c => c.RequestedQuoteId!.Value, c => c.Id, cancellationToken);

        var sourceFormKeys = await _dbContext.FormSubmissions
            .AsNoTracking()
            .Where(s => s.RequestedQuoteId != null && quoteIds.Contains(s.RequestedQuoteId.Value))
            .Select(s => new { QuoteId = s.RequestedQuoteId!.Value, s.FormVersion.FormDefinition.Key })
            .ToListAsync(cancellationToken);
        var sourceFormKeyByQuoteId = sourceFormKeys
            .GroupBy(s => s.QuoteId)
            .ToDictionary(g => g.Key, g => g.First().Key);

        return new PocketBaseListResponse<RequestedQuoteRecordDto>
        {
            Page = page,
            PerPage = perPage,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = quotes
                .Select(q => QuoteMapper.ToRecordDto(
                    q,
                    promotedListingIdByQuoteId.TryGetValue(q.Id, out var listingId) ? listingId : (Guid?)null,
                    sourceFormKeyByQuoteId.GetValueOrDefault(q.Id)))
                .ToList(),
        };
    }

    public async Task<RequestedQuoteRecordDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var quoteId))
        {
            return null;
        }

        var quote = await _dbContext.RequestedQuotes
            .AsNoTracking()
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);

        if (quote is null)
        {
            return null;
        }

        var cargoListingId = await _dbContext.CargoListings
            .AsNoTracking()
            .Where(c => c.RequestedQuoteId == quoteId)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var sourceFormKey = await _dbContext.FormSubmissions
            .AsNoTracking()
            .Where(s => s.RequestedQuoteId == quoteId)
            .Select(s => s.FormVersion.FormDefinition.Key)
            .FirstOrDefaultAsync(cancellationToken);

        return QuoteMapper.ToRecordDto(quote, cargoListingId, sourceFormKey);
    }

    private static DateTime ParseDateOrThrow(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new QuoteException(
                $"{fieldName} is required and must be a valid date.",
                StatusCodes.Status400BadRequest);
        }

        if (DateTime.TryParse(
                value,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return parsed;
        }

        if (DateTimeOffset.TryParse(value, out var offsetParsed))
        {
            return offsetParsed.UtcDateTime;
        }

        throw new QuoteException(
            $"{fieldName} is required and must be a valid date.",
            StatusCodes.Status400BadRequest,
            value);
    }
}
