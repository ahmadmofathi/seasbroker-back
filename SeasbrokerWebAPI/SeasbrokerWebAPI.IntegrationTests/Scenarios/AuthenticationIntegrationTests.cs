using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Seasbroker.Modules.Identity.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthenticationIntegrationTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public AuthenticationIntegrationTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SuperuserPocketBaseLogin_ReturnsJwt()
    {
        var token = await _fixture.Client.LoginSuperuserAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task SuperuserPocketBaseLogin_RejectsInvalidCredentials()
    {
        var response = await _fixture.Client.PostAsJsonAsync(
            "/api/collections/_superusers/auth-with-password",
            new PocketBaseLoginRequest
            {
                Identity = IntegrationTestDefaults.SuperuserEmail,
                Password = "wrong-password-value",
            },
            IntegrationJson.Options);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PocketBaseLogin_RejectsNonSuperuserAccount()
    {
        var email = UniqueTestData.Email("regular");
        const string password = "password123";

        await TestDataQueries.CreateRegularUserAsync(_fixture.Factory.Services, email, password);

        var response = await _fixture.Client.PostAsJsonAsync(
            "/api/collections/_superusers/auth-with-password",
            new PocketBaseLoginRequest { Identity = email, Password = password },
            IntegrationJson.Options);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthLoginRefreshLogout_WorksForRegularUser()
    {
        var email = UniqueTestData.Email("auth");
        const string password = "password123";

        await TestDataQueries.CreateRegularUserAsync(_fixture.Factory.Services, email, password);

        var login = await _fixture.Client.LoginAsync(email, password);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(login.RefreshToken));

        var refreshResponse = await _fixture.Client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshRequest { RefreshToken = login.RefreshToken },
            IntegrationJson.Options);

        refreshResponse.EnsureSuccessStatusCode();
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(IntegrationJson.Options);
        Assert.NotNull(refreshed);
        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);

        var authenticatedClient = _fixture.CreateAuthenticatedClient(refreshed.AccessToken);
        var logoutResponse = await authenticatedClient.PostAsJsonAsync(
            "/api/auth/logout",
            new LogoutRequest { RefreshToken = refreshed.RefreshToken },
            IntegrationJson.Options);

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
    }

    [Fact]
    public async Task PocketBaseRefresh_ReissuesTokenForSuperuser()
    {
        var token = await _fixture.Client.LoginSuperuserAsync();
        var authenticatedClient = _fixture.CreateAuthenticatedClient(token);

        var response = await authenticatedClient.PostAsync(
            "/api/collections/_superusers/auth-refresh",
            content: null);

        response.EnsureSuccessStatusCode();

        var refreshed = await response.Content.ReadFromJsonAsync<PocketBaseAuthResponse>(IntegrationJson.Options);
        Assert.NotNull(refreshed);
        Assert.False(string.IsNullOrWhiteSpace(refreshed.Token));
    }

    [Fact]
    public async Task ProtectedEndpoint_ReturnsUnauthorized_WithoutToken()
    {
        var response = await _fixture.Client.GetAsync("/api/collections/vessels/records");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
