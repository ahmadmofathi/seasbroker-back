namespace Seasbroker.Modules.Cargo.Application.Queries;

public sealed record GetOpenCargoForMatchingQuery(
    string? DeparturePort = null,
    string? ArrivalPort = null,
    DateTime? DepartureFrom = null,
    DateTime? ArrivalTo = null);
