namespace Seasbroker.Modules.Cargo.Application.Helpers;

public static class PocketBaseFilterParser
{
    public static string? TryParseStatusEquals(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        const string prefix = "status = ";
        var trimmed = filter.Trim();

        if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        return trimmed[prefix.Length..].Trim().Trim('"');
    }

    public static string? TryParseCustomerEquals(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        const string prefix = "customer = ";
        var trimmed = filter.Trim();

        if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        return trimmed[prefix.Length..].Trim().Trim('"');
    }
}
