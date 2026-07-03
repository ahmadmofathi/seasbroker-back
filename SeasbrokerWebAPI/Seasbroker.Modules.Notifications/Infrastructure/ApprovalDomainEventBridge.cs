using Seasbroker.Modules.Approval.Application.Abstractions;
using Seasbroker.Modules.Approval.Application.Events;
using Seasbroker.Modules.Notifications.Application.EventHandlers;

namespace Seasbroker.Modules.Notifications.Infrastructure;

public class ApprovalDomainEventBridge : IDomainEventDispatcher
{
    private readonly IMatchNotificationEventHandler _eventHandler;

    public ApprovalDomainEventBridge(IMatchNotificationEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        switch (domainEvent)
        {
            case MatchApprovedEvent approved:
                await _eventHandler.HandleApprovedAsync(approved, cancellationToken);
                break;
            case MatchRejectedEvent rejected:
                await _eventHandler.HandleRejectedAsync(rejected, cancellationToken);
                break;
            case MatchCancelledEvent cancelled:
                await _eventHandler.HandleCancelledAsync(cancelled, cancellationToken);
                break;
            case MatchCompletedEvent completed:
                await _eventHandler.HandleCompletedAsync(completed, cancellationToken);
                break;
        }
    }
}
