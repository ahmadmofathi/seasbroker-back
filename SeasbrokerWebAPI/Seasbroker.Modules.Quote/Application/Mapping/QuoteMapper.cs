using Seasbroker.Modules.Quote.Application.Constants;
using Seasbroker.Modules.Quote.Application.DTOs;

namespace Seasbroker.Modules.Quote.Application.Mapping;

public static class QuoteMapper
{
    public static CustomerRecordDto ToRecordDto(global::Seasbroker.Infrastructure.Persistence.Entities.Customer customer)
    {
        return new CustomerRecordDto
        {
            Id = customer.Id.ToString(),
            CollectionId = QuoteConstants.CustomersCollectionName,
            CollectionName = QuoteConstants.CustomersCollectionName,
            Created = customer.Created,
            Updated = customer.Updated,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
        };
    }

    public static RequestedQuoteRecordDto ToRecordDto(global::Seasbroker.Infrastructure.Persistence.Entities.RequestedQuote quote)
    {
        return new RequestedQuoteRecordDto
        {
            Id = quote.Id.ToString(),
            CollectionId = QuoteConstants.RequestedQuotesCollectionName,
            CollectionName = QuoteConstants.RequestedQuotesCollectionName,
            Created = quote.Created,
            Updated = quote.Updated,
            Customer = quote.CustomerId.ToString(),
            CargoType = quote.CargoType,
            Weight = quote.Weight,
            DeparturePort = quote.DeparturePort,
            DepartureTime = quote.DepartureTime,
            ArrivalPort = quote.ArrivalPort,
            ArrivalTime = quote.ArrivalTime,
            Dimensions = quote.Dimensions,
            AdditionalInfo = quote.AdditionalInfo,
        };
    }
}
