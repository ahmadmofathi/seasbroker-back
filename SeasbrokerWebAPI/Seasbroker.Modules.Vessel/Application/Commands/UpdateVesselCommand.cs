namespace Seasbroker.Modules.Vessel.Application.Commands;

public sealed record UpdateVesselCommand(
    string VesselId,
    string? Name,
    string? VesselType,
    double? Dwt,
    int? TeuCapacity,
    double? LengthOverall,
    double? Beam,
    double? Draft,
    string? CurrentPort,
    string? FlagCountry,
    string? Status,
    string? CustomerId,
    string? Notes);
