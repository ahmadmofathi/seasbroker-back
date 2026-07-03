namespace Seasbroker.Modules.Chat.Infrastructure.Options;

public class GeoLocationOptions
{
    public const string SectionName = "GeoLocation";

    public string BaseUrl { get; set; } = "https://ipgeolocation.abstractapi.com/v1";

    public string ApiKey { get; set; } = string.Empty;
}
