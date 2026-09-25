using System.Globalization;
using System.Text.Json;
using Seasbroker.Modules.Vessel.Application.DTOs;

namespace Seasbroker.Modules.Vessel.Application.Helpers;

public sealed record ShipRequestVessel(
    string Name,
    string? ImoNumber,
    string? VesselType,
    double Dwt,
    string CurrentPort,
    string? FlagCountry,
    double? LengthOverall,
    double? Beam,
    double? Draft,
    int? TeuCapacity);

public sealed record ShipRequestAvailability(
    DateTime AvailableFrom,
    DateTime AvailableTo,
    List<RouteStopDto> RouteStops,
    string? DestinationPort);

/// <summary>
/// Reads a Ship Brokerage form submission (field key to raw value) into a vessel and, when the
/// request says where and when the vessel is available, an availability window with its route.
/// </summary>
public static class ShipRequestMapper
{
    /// <summary>Default length of an open-vessel window when the request gives no end date.</summary>
    public const int DefaultOpenWindowDays = 30;

    /// <summary>Form vessel types mapped to the fleet's vessel types (VesselConstants.AllowedVesselTypes).</summary>
    public static readonly IReadOnlyDictionary<string, string> VesselTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Bulk Carrier"] = "Bulk",
            ["General Cargo or Multipurpose"] = "General Cargo",
            ["Container Ship"] = "Container",
            ["RoRo or PCTC"] = "RoRo",
            ["Oil Tanker"] = "Tanker",
            ["Chemical Tanker"] = "Tanker",
            ["Product Tanker"] = "Tanker",
            ["LNG Carrier"] = "LNG",
            ["LPG Carrier"] = "LPG",
        };

    public static ShipRequestVessel MapVessel(
        IReadOnlyDictionary<string, string?> values,
        string? fallbackType,
        double fallbackDwt,
        string? fallbackPort)
    {
        var formType = Get(values, "vesselType") ?? fallbackType;
        return new ShipRequestVessel(
            Name: Get(values, "vesselName") ?? string.Empty,
            ImoNumber: Get(values, "imoNumber"),
            VesselType: formType is not null && VesselTypes.TryGetValue(formType, out var type) ? type : null,
            Dwt: Number(values, "dwt") ?? fallbackDwt,
            CurrentPort: Get(values, "currentOpenPort") ?? fallbackPort ?? string.Empty,
            FlagCountry: Get(values, "flag"),
            LengthOverall: Number(values, "loa"),
            Beam: Number(values, "beam"),
            Draft: Number(values, "draft") ?? Number(values, "contMaxDraft"),
            TeuCapacity: Number(values, "contCapacityTeu") is { } teu ? (int)teu : null);
    }

    /// <summary>
    /// Open Vessel: one stop at the open port on the open date, window until "Open Date To" (or 30 days).
    /// Scheduled Route: the route as entered (or the older departure/destination fields), window from
    /// the first ETA to the last. Returns null when the request has no usable port and date.
    /// </summary>
    public static ShipRequestAvailability? MapAvailability(IReadOnlyDictionary<string, string?> values, string currentPort)
    {
        var openTo = Date(values, "openDateTo");

        if (string.Equals(Get(values, "availabilityType"), "Scheduled Route", StringComparison.OrdinalIgnoreCase))
        {
            var stops = ScheduledStops(values);
            if (stops.Count == 0)
            {
                return null;
            }

            var from = stops[0].Eta;
            var lastEta = stops[^1].Eta;
            var to = openTo is { } t && t > lastEta ? t : lastEta;
            if (to <= from)
            {
                to = from.AddDays(DefaultOpenWindowDays);
            }

            return new ShipRequestAvailability(from, to, stops, null);
        }

        var openDate = Date(values, "openDate") ?? Date(values, "openDateFrom");
        if (openDate is not { } start)
        {
            return null;
        }

        var end = openTo is { } o && o > start ? o : start.AddDays(DefaultOpenWindowDays);
        var port = Get(values, "openPort") ?? currentPort;
        if (string.IsNullOrWhiteSpace(port))
        {
            return null;
        }

        return new ShipRequestAvailability(
            start,
            end,
            new List<RouteStopDto> { new() { Port = port, Eta = start } },
            Get(values, "preferredDestinationArea"));
    }

    private static List<RouteStopDto> ScheduledStops(IReadOnlyDictionary<string, string?> values)
    {
        var route = Get(values, "schedRoute");
        if (route is not null)
        {
            try
            {
                using var doc = JsonDocument.Parse(route);
                return doc.RootElement.EnumerateArray()
                    .Select(e => (
                        Port: e.TryGetProperty("port", out var p) ? p.GetString() : null,
                        Eta: e.TryGetProperty("eta", out var d) ? ParseDate(d.GetString()) : null))
                    .Where(s => !string.IsNullOrWhiteSpace(s.Port) && s.Eta is not null)
                    .Select(s => new RouteStopDto { Port = s.Port!.Trim(), Eta = s.Eta!.Value })
                    .ToList();
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException)
            {
                return new List<RouteStopDto>();
            }
        }

        // Requests sent before the route builder existed.
        var stops = new List<RouteStopDto>();
        if (Get(values, "schedDeparturePort") is { } depPort && Date(values, "schedDepartureDate") is { } depDate)
        {
            stops.Add(new RouteStopDto { Port = depPort, Eta = depDate });
        }

        if (Get(values, "schedDestinationPort") is { } destPort && Date(values, "schedEta") is { } eta)
        {
            stops.Add(new RouteStopDto { Port = destPort, Eta = eta });
        }

        return stops;
    }

    private static string? Get(IReadOnlyDictionary<string, string?> values, string key) =>
        values.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : null;

    private static double? Number(IReadOnlyDictionary<string, string?> values, string key) =>
        double.TryParse(Get(values, key), NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : null;

    private static DateTime? Date(IReadOnlyDictionary<string, string?> values, string key) => ParseDate(Get(values, key));

    private static DateTime? ParseDate(string? value) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d)
            ? d
            : null;
}
