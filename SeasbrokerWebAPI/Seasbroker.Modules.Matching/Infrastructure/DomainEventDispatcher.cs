using Microsoft.Extensions.Logging;
using Seasbroker.Modules.Matching.Application.Abstractions;

namespace Seasbroker.Modules.Matching.Infrastructure;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(ILogger<DomainEventDispatcher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        _logger.LogInformation("Domain event published: {EventType}", typeof(TEvent).Name);
        return Task.CompletedTask;
    }
}
