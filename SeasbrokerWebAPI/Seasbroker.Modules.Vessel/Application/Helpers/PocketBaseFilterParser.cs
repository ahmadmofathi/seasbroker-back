namespace Seasbroker.Modules.Vessel.Application.Helpers;

public static class PocketBaseFilterParser
{
    public static string? TryParseVesselIdEquals(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        const string prefix = "vesselId = ";
        var trimmed = filter.Trim();

        if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        return trimmed[prefix.Length..].Trim().Trim('"');
    }
}
