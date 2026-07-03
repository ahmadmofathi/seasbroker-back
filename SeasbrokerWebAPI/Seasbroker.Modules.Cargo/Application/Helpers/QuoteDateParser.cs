using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Cargo.Application.Exceptions;

namespace Seasbroker.Modules.Cargo.Application.Helpers;

public static class QuoteDateParser
{
    public static DateTime ParseOrThrow(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new CargoException(
                $"Unable to parse {fieldName} from quote.",
                StatusCodes.Status400BadRequest);
        }

        if (DateTime.TryParse(
                value,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return parsed;
        }

        if (DateTimeOffset.TryParse(value, out var offsetParsed))
        {
            return offsetParsed.UtcDateTime;
        }

        throw new CargoException(
            $"Unable to parse {fieldName} from quote.",
            StatusCodes.Status400BadRequest,
            value);
    }
}
