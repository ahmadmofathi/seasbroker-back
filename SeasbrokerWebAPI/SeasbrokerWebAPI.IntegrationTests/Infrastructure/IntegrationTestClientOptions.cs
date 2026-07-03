using Microsoft.AspNetCore.Mvc.Testing;

namespace SeasbrokerWebAPI.IntegrationTests.Infrastructure;

internal static class IntegrationTestClientOptions
{
    public static WebApplicationFactoryClientOptions Create() => new()
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost"),
    };
}
