namespace Seasbroker.Modules.Vessel.Application.Constants;

public static class VesselConstants
{
    public const string SuperuserRole = "Superuser";

    public const string SuperuserPolicy = "Superuser";

    public const string VesselsCollectionName = "vessels";

    public const string VesselAvailabilitiesCollectionName = "vesselAvailabilities";

    public const int MaxAvailabilityWindowDays = 365;

    public static readonly IReadOnlyList<string> AllowedVesselTypes =
    [
        "Bulk",
        "Container",
        "RoRo",
        "Tanker",
        "General Cargo",
        "LNG",
        "LPG",
    ];
}
