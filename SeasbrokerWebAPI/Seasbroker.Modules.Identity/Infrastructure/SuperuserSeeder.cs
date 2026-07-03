using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Identity.Application.Constants;
using Seasbroker.Modules.Identity.Infrastructure.Options;

namespace Seasbroker.Modules.Identity.Infrastructure;

public class SuperuserSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SuperuserSeedOptions _options;
    private readonly ILogger<SuperuserSeeder> _logger;

    public SuperuserSeeder(
        IServiceScopeFactory scopeFactory,
        IOptions<SuperuserSeedOptions> options,
        ILogger<SuperuserSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Password))
        {
            _logger.LogWarning("Superuser seed skipped: Identity:Superuser credentials are not configured.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var utcNow = DateTime.UtcNow;

        if (!await roleManager.RoleExistsAsync(SeasbrokerIdentityConstants.SuperuserRole))
        {
            var role = new Role
            {
                Name = SeasbrokerIdentityConstants.SuperuserRole,
                NormalizedName = SeasbrokerIdentityConstants.SuperuserRole.ToUpperInvariant(),
                Created = utcNow,
                Updated = utcNow,
            };

            var roleResult = await roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
            {
                _logger.LogError(
                    "Failed to seed Superuser role: {Errors}",
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                return;
            }

            _logger.LogInformation("Seeded role '{Role}'.", SeasbrokerIdentityConstants.SuperuserRole);
        }

        var existingUser = await userManager.FindByEmailAsync(_options.Email);
        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, SeasbrokerIdentityConstants.SuperuserRole))
            {
                await userManager.AddToRoleAsync(existingUser, SeasbrokerIdentityConstants.SuperuserRole);
            }

            return;
        }

        var user = new User
        {
            Email = _options.Email,
            UserName = _options.Email,
            NormalizedEmail = _options.Email.ToUpperInvariant(),
            NormalizedUserName = _options.Email.ToUpperInvariant(),
            Verified = true,
            EmailConfirmed = true,
            Created = utcNow,
            Updated = utcNow,
        };

        var createResult = await userManager.CreateAsync(user, _options.Password);
        if (!createResult.Succeeded)
        {
            _logger.LogError(
                "Failed to seed superuser: {Errors}",
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, SeasbrokerIdentityConstants.SuperuserRole);
        _logger.LogInformation("Seeded superuser account '{Email}'.", _options.Email);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
