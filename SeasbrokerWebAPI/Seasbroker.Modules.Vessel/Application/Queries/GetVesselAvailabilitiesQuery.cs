namespace Seasbroker.Modules.Vessel.Application.Queries;

public sealed record GetVesselAvailabilitiesQuery(string VesselId, bool ActiveOnly = false);
