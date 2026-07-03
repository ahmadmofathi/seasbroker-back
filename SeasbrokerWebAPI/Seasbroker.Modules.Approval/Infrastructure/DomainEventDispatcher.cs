using Microsoft.Extensions.Logging;
using Seasbroker.Modules.Approval.Application.Abstractions;

namespace Seasbroker.Modules.Approval.Infrastructure;

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
