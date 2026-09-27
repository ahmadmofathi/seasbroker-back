using Microsoft.Extensions.Logging.Abstractions;
using Seasbroker.Infrastructure.Persistence;

namespace Seasbroker.Modules.Forms.Tests;

public class ResilientStartupTaskTests
{
    private sealed class FlakyTask(int failuresBeforeSuccess) : ResilientStartupTask(NullLogger.Instance)
    {
        public int Attempts { get; private set; }
        public TaskCompletionSource Succeeded { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override string Name => "Flaky test task";

        protected override IReadOnlyList<TimeSpan> RetryDelays =>
            Enumerable.Repeat(TimeSpan.FromMilliseconds(10), 5).ToArray();

        protected override Task RunOnceAsync(CancellationToken cancellationToken)
        {
            Attempts++;
            if (Attempts <= failuresBeforeSuccess)
            {
                throw new TimeoutException("database not answering yet");
            }

            Succeeded.TrySetResult();
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Healthy_Database_Runs_Once_During_Startup()
    {
        using var task = new FlakyTask(failuresBeforeSuccess: 0);

        await task.StartAsync(CancellationToken.None);

        Assert.Equal(1, task.Attempts);
        Assert.True(task.Succeeded.Task.IsCompleted);
    }

    [Fact]
    public async Task Startup_Failure_Does_Not_Stop_The_App_And_Is_Retried()
    {
        using var task = new FlakyTask(failuresBeforeSuccess: 2);

        await task.StartAsync(CancellationToken.None); // must not throw

        await task.Succeeded.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(3, task.Attempts);
        await task.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Stopping_The_App_Cancels_Pending_Retries()
    {
        using var task = new FlakyTask(failuresBeforeSuccess: int.MaxValue);

        await task.StartAsync(CancellationToken.None);
        await task.StopAsync(CancellationToken.None);
        var attemptsAtStop = task.Attempts;

        await Task.Delay(100);
        Assert.Equal(attemptsAtStop, task.Attempts);
    }
}
