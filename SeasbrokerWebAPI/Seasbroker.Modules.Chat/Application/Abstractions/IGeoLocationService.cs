namespace Seasbroker.Modules.Chat.Application.Abstractions;

public interface IGeoLocationService
{
    Task<string> GetCityCountryAsync(string ip, CancellationToken cancellationToken = default);
}
