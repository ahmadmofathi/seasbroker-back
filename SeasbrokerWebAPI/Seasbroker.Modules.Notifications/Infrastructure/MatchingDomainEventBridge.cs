using Seasbroker.Modules.Matching.Application.Abstractions;
using Seasbroker.Modules.Matching.Application.Events;
using Seasbroker.Modules.Notifications.Application.EventHandlers;

namespace Seasbroker.Modules.Notifications.Infrastructure;

public class MatchingDomainEventBridge : IDomainEventDispatcher
{
    private readonly IMatchNotificationEventHandler _eventHandler;

    public MatchingDomainEventBridge(IMatchNotificationEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        if (domainEvent is MatchPendingApprovalEvent pendingApproval)
        {
            await _eventHandler.HandlePendingApprovalAsync(pendingApproval, cancellationToken);
        }
    }
}
