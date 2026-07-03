namespace Seasbroker.Modules.Vessel.Application.Queries;

public sealed record GetAvailableVesselsQuery(
    string? OpenPort = null,
    DateTime? AvailableFrom = null,
    DateTime? AvailableTo = null);
