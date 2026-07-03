namespace Seasbroker.Modules.Cargo.Application.Commands;

public sealed record CreateCargoListingCommand(
    string CustomerId,
    string? RequestedQuoteId,
    string? ReferenceNumber,
    string CargoType,
    double Weight,
    string Dimensions,
    string DeparturePort,
    DateTime DepartureTime,
    string ArrivalPort,
    DateTime ArrivalTime,
    string? AdditionalInfo,
    string? Status,
    int? Priority);
