namespace Seasbroker.Modules.Vessel.Application.Commands;

public sealed record CreateVesselCommand(
    string Name,
    string? ImoNumber,
    string VesselType,
    double Dwt,
    int? TeuCapacity,
    double? LengthOverall,
    double? Beam,
    double? Draft,
    string CurrentPort,
    string? FlagCountry,
    string? Status,
    string? CustomerId,
    string? Notes);
