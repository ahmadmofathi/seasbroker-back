namespace Seasbroker.Modules.Vessel.Application.Commands;

public sealed record UpdateVesselAvailabilityCommand(
    string AvailabilityId,
    DateTime? AvailableFrom,
    DateTime? AvailableTo,
    string? OpenPort,
    string? DestinationPort,
    bool? IsActive);
