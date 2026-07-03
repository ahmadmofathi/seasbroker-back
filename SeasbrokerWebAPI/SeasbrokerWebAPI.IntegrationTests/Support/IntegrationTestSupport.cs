using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Identity.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;

namespace SeasbrokerWebAPI.IntegrationTests.Support;

internal static class IntegrationJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web);
}

internal static class HttpClientExtensions
{
    public static HttpClient CreateAuthenticatedClient(this SqlServerIntegrationFixture fixture, string accessToken)
    {
        var client = fixture.Factory.CreateClient(IntegrationTestClientOptions.Create());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public static async Task<string> LoginSuperuserAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/collections/_superusers/auth-with-password",
            new PocketBaseLoginRequest
            {
                Identity = IntegrationTestDefaults.SuperuserEmail,
                Password = IntegrationTestDefaults.SuperuserPassword,
            },
            IntegrationJson.Options);

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<PocketBaseAuthResponse>(IntegrationJson.Options);
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));

        return auth.Token;
    }

    public static async Task<AuthResponse> LoginAsync(this HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest { Email = email, Password = password },
            IntegrationJson.Options);

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(IntegrationJson.Options);
        Assert.NotNull(auth);

        return auth;
    }
}

internal static class TestDataQueries
{
    public static async Task<Guid> GetLatestQuoteIdForEmailAsync(IServiceProvider services, string email)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SeasbrokerDbContext>();

        var customer = await dbContext.Customers
            .AsNoTracking()
            .SingleAsync(c => c.Email == email);

        var quote = await dbContext.RequestedQuotes
            .AsNoTracking()
            .Where(q => q.CustomerId == customer.Id)
            .OrderByDescending(q => q.Created)
            .FirstAsync();

        return quote.Id;
    }

    public static async Task<Guid> GetCustomerIdForEmailAsync(IServiceProvider services, string email)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SeasbrokerDbContext>();

        return await dbContext.Customers
            .AsNoTracking()
            .Where(c => c.Email == email)
            .Select(c => c.Id)
            .SingleAsync();
    }

    public static async Task<Guid> CreateRegularUserAsync(
        IServiceProvider services,
        string email,
        string password)
    {
        await using var scope = services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = new User
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, password);
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));

        return user.Id;
    }
}

internal static class UniqueTestData
{
    public static string Email(string prefix) => $"{prefix}-{Guid.NewGuid():N}@integration.seasbroker.test";

    public static string ReferenceNumber(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
}
