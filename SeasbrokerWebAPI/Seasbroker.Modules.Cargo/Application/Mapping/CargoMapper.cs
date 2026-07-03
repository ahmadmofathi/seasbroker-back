using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.Constants;
using Seasbroker.Modules.Cargo.Application.DTOs;

namespace Seasbroker.Modules.Cargo.Application.Mapping;

public static class CargoMapper
{
    public static CargoListingRecordDto ToRecordDto(CargoListing listing)
    {
        return new CargoListingRecordDto
        {
            Id = listing.Id.ToString(),
            CollectionId = CargoConstants.CargoListingsCollectionName,
            CollectionName = CargoConstants.CargoListingsCollectionName,
            Created = listing.Created,
            Updated = listing.Updated,
            Customer = listing.CustomerId.ToString(),
            RequestedQuote = listing.RequestedQuoteId?.ToString(),
            ReferenceNumber = listing.ReferenceNumber,
            CargoType = listing.CargoType,
            Weight = listing.Weight,
            Dimensions = listing.Dimensions,
            DeparturePort = listing.DeparturePort,
            DepartureTime = listing.DepartureTime,
            ArrivalPort = listing.ArrivalPort,
            ArrivalTime = listing.ArrivalTime,
            AdditionalInfo = listing.AdditionalInfo,
            Status = listing.Status,
            Priority = listing.Priority,
        };
    }
}
