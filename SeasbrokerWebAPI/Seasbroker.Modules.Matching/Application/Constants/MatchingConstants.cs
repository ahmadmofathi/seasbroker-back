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

    public static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> CargoVesselTypeCompatibility =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Bulk"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Bulk", "General Cargo" },
            ["Container"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Container" },
            ["RoRo"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "RoRo", "General Cargo" },
            ["Tanker"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Tanker", "LNG", "LPG" },
            ["General Cargo"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "General Cargo", "Bulk", "RoRo" },
            ["LNG"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "LNG", "Tanker" },
            ["LPG"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "LPG", "Tanker" },
        };
}
