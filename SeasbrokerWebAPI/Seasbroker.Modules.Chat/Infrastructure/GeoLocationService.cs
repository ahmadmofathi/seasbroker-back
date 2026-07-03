using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seasbroker.Modules.Chat.Application.Abstractions;
using Seasbroker.Modules.Chat.Infrastructure.Options;

namespace Seasbroker.Modules.Chat.Infrastructure;

public class GeoLocationService : IGeoLocationService
{
    private readonly HttpClient _httpClient;
    private readonly GeoLocationOptions _options;
    private readonly ILogger<GeoLocationService> _logger;

    public GeoLocationService(
        HttpClient httpClient,
        IOptions<GeoLocationOptions> options,
        ILogger<GeoLocationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetCityCountryAsync(string ip, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("GeoLocation API key is not configured.");
            return string.Empty;
        }

        var url =
            $"{_options.BaseUrl}?api_key={Uri.EscapeDataString(_options.ApiKey)}&fields=country,city&ip={Uri.EscapeDataString(ip)}";

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GeoLocation API returned status code {StatusCode}", response.StatusCode);
                return string.Empty;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var data = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream, cancellationToken: cancellationToken);

            if (data is null ||
                !data.TryGetValue("city", out var city) ||
                !data.TryGetValue("country", out var country))
            {
                return string.Empty;
            }

            return $"{city}, {country}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error making GeoLocation request.");
            return string.Empty;
        }
    }
}
