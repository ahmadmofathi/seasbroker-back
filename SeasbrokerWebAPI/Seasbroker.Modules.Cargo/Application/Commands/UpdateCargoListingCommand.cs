namespace Seasbroker.Modules.Cargo.Application.Commands;

public sealed record UpdateCargoListingCommand(
    string CargoListingId,
    string? CargoType,
    double? Weight,
    string? Dimensions,
    string? DeparturePort,
    DateTime? DepartureTime,
    string? ArrivalPort,
    DateTime? ArrivalTime,
    string? AdditionalInfo,
    int? Priority);
