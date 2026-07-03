using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MsSql;

namespace SeasbrokerWebAPI.IntegrationTests.Infrastructure;

public sealed class SqlServerIntegrationFixture : IAsyncLifetime
{
    private const string ConnectionStringEnvironmentVariable = "SEASBROKER_TEST_CONNECTION_STRING";

    private MsSqlContainer? _sqlContainer;
    private string _connectionString = string.Empty;

    public SeasbrokerWebApplicationFactory Factory { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _connectionString = await ResolveConnectionStringAsync();
        Factory = new SeasbrokerWebApplicationFactory(_connectionString);
        Client = Factory.CreateClient(IntegrationTestClientOptions.Create());

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        if (_sqlContainer is not null)
        {
            await _sqlContainer.DisposeAsync();
        }
    }

    private async Task<string> ResolveConnectionStringAsync()
    {
        var configured = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        if (ShouldUseTestcontainers())
        {
            _sqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .Build();

            await _sqlContainer.StartAsync();
            return _sqlContainer.GetConnectionString();
        }

        var databaseName = $"Seasbroker_Integration_{Guid.NewGuid():N}";
        return $"Server=(localdb)\\mssqllocaldb;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
    }

    private static bool ShouldUseTestcontainers()
    {
        var preference = Environment.GetEnvironmentVariable("SEASBROKER_USE_TESTCONTAINERS");
        if (string.Equals(preference, "false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(preference, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return File.Exists("/var/run/docker.sock") ||
               OperatingSystem.IsWindows() && IsDockerDesktopRunning();
    }

    private static bool IsDockerDesktopRunning()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("docker", "info")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return false;
            }

            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
