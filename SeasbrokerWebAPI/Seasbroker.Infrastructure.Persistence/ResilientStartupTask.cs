using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Seasbroker.Infrastructure.Persistence;

/// <summary>
/// A startup job that touches the database (seeding, upgrades). The first attempt still runs
/// during startup, so on a healthy database everything is in place before the first request -
/// exactly as before. But if it fails (e.g. the remote SQL Server is slow to accept logins right
/// after a restart), the failure is logged instead of stopping the whole API, and the job is
/// retried in the background with increasing delays.
/// </summary>
public abstract class ResilientStartupTask : IHostedService, IDisposable
{
    private static readonly TimeSpan[] DefaultRetryDelays =
    {
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(10),
    };

    private readonly ILogger _logger;
    private readonly CancellationTokenSource _stopping = new();
    private Task? _retries;

    protected ResilientStartupTask(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>Waits before each background retry; about 18 minutes in total by default.</summary>
    protected virtual IReadOnlyList<TimeSpan> RetryDelays => DefaultRetryDelays;

    /// <summary>Short name for log messages, e.g. "Form seeding".</summary>
    protected abstract string Name { get; }

    /// <summary>The job itself. Must be safe to run again after a partial failure.</summary>
    protected abstract Task RunOnceAsync(CancellationToken cancellationToken);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await RunOnceAsync(cancellationToken);
            return;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "{Task} failed during startup; the API will start anyway and retry it in the background.", Name);
        }

        _retries = RetryInBackgroundAsync(_stopping.Token);
    }

    private async Task RetryInBackgroundAsync(CancellationToken stoppingToken)
    {
        var delays = RetryDelays;
        for (var attempt = 0; attempt < delays.Count; attempt++)
        {
            try
            {
                await Task.Delay(delays[attempt], stoppingToken);
                await RunOnceAsync(stoppingToken);
                _logger.LogInformation("{Task} succeeded on retry {Attempt}.", Name, attempt + 1);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Task} retry {Attempt} of {Total} failed.", Name, attempt + 1, delays.Count);
            }
        }

        _logger.LogError("{Task} kept failing; giving up until the next restart.", Name);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stopping.Cancel();
        if (_retries is not null)
        {
            await Task.WhenAny(_retries, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }

    public void Dispose()
    {
        _stopping.Dispose();
        GC.SuppressFinalize(this);
    }
}
