using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Seasbroker.Infrastructure.Persistence;

namespace SeasbrokerWebAPI.IntegrationTests.Infrastructure;

public sealed class SeasbrokerWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public SeasbrokerWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _connectionString,
                ["Identity:Superuser:Email"] = IntegrationTestDefaults.SuperuserEmail,
                ["Identity:Superuser:Password"] = IntegrationTestDefaults.SuperuserPassword,
                ["GeoLocation:ApiKey"] = string.Empty,
                ["Matching:ExpiryWorkerIntervalMinutes"] = "60",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.Insert(0, ServiceDescriptor.Singleton<IHostedService, IntegrationTestDatabaseBootstrapper>());
            services.AddSingleton<IStartupFilter, IntegrationTestJwtAuthenticationStartupFilter>();
        });
    }

    private sealed class IntegrationTestDatabaseBootstrapper : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public IntegrationTestDatabaseBootstrapper(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SeasbrokerDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    /// <summary>
    /// WebApplicationFactory does not always execute <c>UseAuthentication</c> from Program before endpoints.
    /// This filter mirrors the default authentication middleware for JWT bearer tokens in tests.
    /// </summary>
    private sealed class IntegrationTestJwtAuthenticationStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    var authenticateResult = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                    if (authenticateResult.Succeeded && authenticateResult.Principal is not null)
                    {
                        context.User = authenticateResult.Principal;
                    }

                    await nextMiddleware();
                });

                next(app);
            };
        }
    }
}
