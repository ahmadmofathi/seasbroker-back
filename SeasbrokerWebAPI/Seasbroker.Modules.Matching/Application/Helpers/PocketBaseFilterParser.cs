namespace Seasbroker.Modules.Matching.Application.Helpers;

public static class PocketBaseFilterParser
{
    public static string? TryParseEquals(string? filter, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        var prefix = $"{fieldName} = ";
        var trimmed = filter.Trim();

        if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        return trimmed[prefix.Length..].Trim().Trim('"');
    }
}
