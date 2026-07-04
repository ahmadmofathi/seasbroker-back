using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Identity.Application.Constants;
using Seasbroker.Modules.Identity.Application.Services;
using Seasbroker.Modules.Identity.Infrastructure.Options;

namespace Seasbroker.Modules.Identity.Tests;

public class SuperuserSeedOptionsValidatorTests
{
    [Fact]
    public void Validate_Production_RejectsWeakPassword()
    {
        var environment = CreateEnvironment(isDevelopment: false);
        var validator = new SuperuserSeedOptionsValidator(environment);

        var result = validator.Validate(
            null,
            new SuperuserSeedOptions { Email = "admin@example.com", Password = "adminadmin" });

        Assert.True(result.Failed);
        Assert.Contains("default weak value", result.FailureMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_Production_RejectsShortPassword()
    {
        var environment = CreateEnvironment(isDevelopment: false);
        var validator = new SuperuserSeedOptionsValidator(environment);

        var result = validator.Validate(
            null,
            new SuperuserSeedOptions { Email = "admin@example.com", Password = "short" });

        Assert.True(result.Failed);
        Assert.Contains("12 characters", result.FailureMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_Production_AcceptsStrongPassword()
    {
        var environment = CreateEnvironment(isDevelopment: false);
        var validator = new SuperuserSeedOptionsValidator(environment);

        var result = validator.Validate(
            null,
            new SuperuserSeedOptions { Email = "admin@example.com", Password = "Production_Strong_12!" });

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_Development_RequiresConfiguredCredentials()
    {
        var environment = CreateEnvironment(isDevelopment: true);
        var validator = new SuperuserSeedOptionsValidator(environment);

        var result = validator.Validate(null, new SuperuserSeedOptions());

        Assert.True(result.Failed);
    }

    private static IHostEnvironment CreateEnvironment(bool isDevelopment)
    {
        var mock = new Mock<IHostEnvironment>();
        mock.SetupGet(e => e.EnvironmentName)
            .Returns(isDevelopment ? Environments.Development : Environments.Production);
        return mock.Object;
    }
}

public class AuthServiceSecurityTests
{
    private static User CreateTestUser(string email) =>
        new()
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
        };

    [Fact]
    public async Task LoginAsync_LocksOut_AfterRepeatedFailedAttempts()
    {
        await using var provider = await CreateServiceProviderAsync();
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var user = CreateTestUser("user@example.com");
        var createResult = await userManager.CreateAsync(user, "correct-password");
        Assert.True(createResult.Succeeded, string.Join(", ", createResult.Errors.Select(e => e.Description)));
        await userManager.SetLockoutEnabledAsync(user, true);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.LoginAsync("user@example.com", "wrong-password"));
        }

        var lockedOut = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginAsync("user@example.com", "wrong-password"));

        Assert.Contains("locked", lockedOut.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginPocketBaseAsync_RejectsUserWithoutSuperuserRole()
    {
        await using var provider = await CreateServiceProviderAsync();
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var user = CreateTestUser("regular@example.com");
        await userManager.CreateAsync(user, "password123");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginPocketBaseAsync("regular@example.com", "password123"));
    }

    [Fact]
    public async Task LoginPocketBaseAsync_AllowsSuperuser()
    {
        await using var provider = await CreateServiceProviderAsync();
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        await roleManager.CreateAsync(new Role
        {
            Name = SeasbrokerIdentityConstants.SuperuserRole,
            NormalizedName = SeasbrokerIdentityConstants.SuperuserRole.ToUpperInvariant(),
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
        });

        var user = CreateTestUser("super@example.com");
        var createResult = await userManager.CreateAsync(user, "password123");
        Assert.True(createResult.Succeeded);

        var persistedUser = await userManager.FindByEmailAsync("super@example.com");
        Assert.NotNull(persistedUser);
        await userManager.AddToRoleAsync(persistedUser, SeasbrokerIdentityConstants.SuperuserRole);

        var response = await authService.LoginPocketBaseAsync("super@example.com", "password123");

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task RefreshPocketBaseAsync_RejectsUserWithoutSuperuserRole()
    {
        await using var provider = await CreateServiceProviderAsync();
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var user = CreateTestUser("regular@example.com");
        await userManager.CreateAsync(user, "password123");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.RefreshPocketBaseAsync(user.Id));
    }

    [Fact]
    public async Task LogoutAsync_DoesNotRevokeAnotherUsersRefreshToken()
    {
        await using var provider = await CreateServiceProviderAsync();
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var userA = CreateTestUser("a@example.com");
        var userB = CreateTestUser("b@example.com");
        Assert.True((await userManager.CreateAsync(userA, "password123")).Succeeded);
        Assert.True((await userManager.CreateAsync(userB, "password123")).Succeeded);

        var tokenForB = await refreshTokenService.CreateAsync(userB.Id);
        await authService.LogoutAsync(userA.Id, tokenForB.Token);

        using var verifyScope = provider.CreateScope();
        var verifyRefreshTokenService = verifyScope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
        var stillActive = await verifyRefreshTokenService.GetActiveAsync(tokenForB.Token);
        Assert.NotNull(stillActive);
    }

    [Fact]
    public async Task LogoutAsync_RevokesOnlyAuthenticatedUsersRefreshToken()
    {
        await using var provider = await CreateServiceProviderAsync();
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var user = CreateTestUser("owner@example.com");
        await userManager.CreateAsync(user, "password123");

        var token = await refreshTokenService.CreateAsync(user.Id);
        await authService.LogoutAsync(user.Id, token.Token);

        var revoked = await refreshTokenService.GetActiveAsync(token.Token);
        Assert.Null(revoked);
    }

    private static async Task<ServiceProvider> CreateServiceProviderAsync()
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();

        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddDataProtection();
        services.AddDbContext<SeasbrokerDbContext>(options => options.UseInMemoryDatabase(dbName));
        services
            .AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<SeasbrokerDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = "test";
            options.Audience = "test";
            options.Key = "SeasbrokerDevSigningKey_ChangeInProduction_32chars!";
            options.AccessTokenExpiryMinutes = 60;
            options.RefreshTokenExpiryDays = 7;
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<SeasbrokerDbContext>().Database.EnsureCreatedAsync();

        return provider;
    }
}
