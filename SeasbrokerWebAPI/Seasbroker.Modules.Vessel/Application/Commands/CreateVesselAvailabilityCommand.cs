namespace Seasbroker.Modules.Vessel.Application.Commands;

public sealed record CreateVesselAvailabilityCommand(
    string VesselId,
    DateTime AvailableFrom,
    DateTime AvailableTo,
    string OpenPort,
    string? DestinationPort);
