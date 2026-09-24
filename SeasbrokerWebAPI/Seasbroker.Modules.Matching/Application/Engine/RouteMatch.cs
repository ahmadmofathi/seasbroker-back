using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Modules.Matching.Application.Engine;

/// <summary>
/// Where a cargo's loading and discharge ports fall on a vessel's route, in sailing order.
/// A route is the availability's ordered stops, or - for windows created before routes existed -
/// just OpenPort followed by DestinationPort (no ETAs).
/// </summary>
internal sealed class RouteMatch
{
    private RouteMatch(RouteStopView? loading, RouteStopView? discharge, bool loadsAtCurrentPort)
    {
        Loading = loading;
        Discharge = discharge;
        LoadsAtCurrentPort = loadsAtCurrentPort;
    }

    /// <summary>The first stop at the cargo's loading port, if the vessel calls there.</summary>
    public RouteStopView? Loading { get; }

    /// <summary>A stop at the cargo's discharge port - after the loading stop when there is one.</summary>
    public RouteStopView? Discharge { get; }

    /// <summary>The vessel is sitting at the loading port right now (Vessel.CurrentPort).</summary>
    public bool LoadsAtCurrentPort { get; }

    public bool CanLoad => Loading is not null || LoadsAtCurrentPort;

    public static RouteMatch Find(CargoListing cargo, Vessel vessel, VesselAvailability availability)
    {
        var route = BuildRoute(availability);

        var loadIndex = route.FindIndex(s => PortsEqual(s.Port, cargo.DeparturePort));
        var loadsAtCurrentPort = loadIndex < 0 && PortsEqual(vessel.CurrentPort, cargo.DeparturePort);

        // Discharge has to come after loading; with no loading stop, any stop on the route counts.
        var dischargeIndex = route.FindIndex(Math.Max(loadIndex + 1, 0), s => PortsEqual(s.Port, cargo.ArrivalPort));

        return new RouteMatch(
            loadIndex >= 0 ? route[loadIndex] : null,
            dischargeIndex >= 0 ? route[dischargeIndex] : null,
            loadsAtCurrentPort);
    }

    private static List<RouteStopView> BuildRoute(VesselAvailability availability)
    {
        if (availability.RouteStops is { Count: > 0 } stops)
        {
            return stops.Select(s => new RouteStopView(s.Port, s.Eta)).ToList();
        }

        var legacy = new List<RouteStopView> { new(availability.OpenPort, null) };
        if (!string.IsNullOrWhiteSpace(availability.DestinationPort))
        {
            legacy.Add(new RouteStopView(availability.DestinationPort, null));
        }

        return legacy;
    }

    private static bool PortsEqual(string? left, string? right) =>
        !string.IsNullOrWhiteSpace(left) &&
        !string.IsNullOrWhiteSpace(right) &&
        string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
}

internal sealed record RouteStopView(string Port, DateTime? Eta);
