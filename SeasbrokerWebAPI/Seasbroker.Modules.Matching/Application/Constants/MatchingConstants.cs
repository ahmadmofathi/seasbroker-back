namespace Seasbroker.Modules.Matching.Application.Constants;

public static class MatchingConstants
{
    public const string SuperuserRole = "Superuser";

    public const string SuperuserPolicy = "Superuser";

    public const string MatchesCollectionName = "matches";

    public const string MatchingRulesCollectionName = "matchingRules";

    public const string CriterionPort = "Port";

    public const string CriterionDate = "Date";

    public const string CriterionCapacity = "Capacity";

    public const string CriterionType = "Type";

    public const string CriterionPriority = "Priority";

    /// <summary>
    /// Which vessel types can carry each cargo type. This is a hard rule for automatic matching: a
    /// vessel of any other type is never proposed, however well its ports, dates and capacity fit.
    /// A cargo type that isn't listed (e.g. "Other") gets no automatic matches at all - a broker can
    /// still pair it with a vessel using Manual Match.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> CargoVesselTypeCompatibility =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // Cargo types as the public Cargo Brokerage form submits them (see FormSeedData.CargoTypeOptions).
            ["Dry Bulk"] = Types("Bulk"),
            ["General & Breakbulk Cargo"] = Types("General Cargo"),
            ["Project & Heavy-Lift Cargo"] = Types("General Cargo"),
            ["Containerized Cargo"] = Types("Container"),
            ["RoRo"] = Types("RoRo"),
            ["Liquid Bulk"] = Types("Tanker"),
            ["Gas"] = Types("LNG", "LPG"),
            ["Refrigerated & Perishable Cargo"] = Types("Container"),

            // Older / admin-entered cargo types, which use the vessel type names directly.
            ["Bulk"] = Types("Bulk"),
            ["Container"] = Types("Container"),
            ["Tanker"] = Types("Tanker"),
            ["General Cargo"] = Types("General Cargo"),
            ["LNG"] = Types("LNG"),
            ["LPG"] = Types("LPG"),
        };

    /// <summary>True when a vessel of <paramref name="vesselType"/> can carry <paramref name="cargoType"/>.</summary>
    public static bool IsTypeCompatible(string? cargoType, string? vesselType) =>
        cargoType is not null &&
        vesselType is not null &&
        CargoVesselTypeCompatibility.TryGetValue(cargoType.Trim(), out var vesselTypes) &&
        vesselTypes.Contains(vesselType.Trim());

    private static IReadOnlySet<string> Types(params string[] vesselTypes) =>
        new HashSet<string>(vesselTypes, StringComparer.OrdinalIgnoreCase);
}
