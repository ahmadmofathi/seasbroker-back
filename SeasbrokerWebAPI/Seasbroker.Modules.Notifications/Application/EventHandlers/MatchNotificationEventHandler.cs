using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Approval.Application.Events;
using Seasbroker.Modules.Matching.Application.Events;
using Seasbroker.Modules.Notifications.Application.Queries;
using Seasbroker.Modules.Notifications.Application.Services;

namespace Seasbroker.Modules.Notifications.Application.EventHandlers;

public interface IMatchNotificationEventHandler
{
    Task HandlePendingApprovalAsync(MatchPendingApprovalEvent domainEvent, CancellationToken cancellationToken = default);

    Task HandleApprovedAsync(MatchApprovedEvent domainEvent, CancellationToken cancellationToken = default);

    Task HandleRejectedAsync(MatchRejectedEvent domainEvent, CancellationToken cancellationToken = default);

    Task HandleCancelledAsync(MatchCancelledEvent domainEvent, CancellationToken cancellationToken = default);

    Task HandleCompletedAsync(MatchCompletedEvent domainEvent, CancellationToken cancellationToken = default);
}

public class MatchNotificationEventHandler : IMatchNotificationEventHandler
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;

    public MatchNotificationEventHandler(
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver)
    {
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
    }

    public async Task HandlePendingApprovalAsync(
        MatchPendingApprovalEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var (cargoOwnerId, vesselOwnerId) = await _recipientResolver.ResolveMatchOwnersAsync(
            domainEvent.CargoListingId,
            domainEvent.VesselId,
            cancellationToken);

        var superusers = await _recipientResolver.ResolveSuperuserIdsAsync(cancellationToken);
        var payload = NotificationPayloadBuilder.Build(domainEvent);

        var requests = new List<CreateNotificationRequest>
        {
            BuildRequest(
                cargoOwnerId,
                "Match pending approval",
                "A new vessel match proposal is awaiting broker approval.",
                NotificationType.MatchPendingApproval,
                payload),
        };

        if (vesselOwnerId.HasValue)
        {
            requests.Add(BuildRequest(
                vesselOwnerId.Value,
                "Match pending approval",
                "Your vessel has been proposed for a cargo match awaiting approval.",
                NotificationType.MatchPendingApproval,
                payload));
        }

        foreach (var superuserId in superusers)
        {
            requests.Add(BuildRequest(
                superuserId,
                "Match pending approval",
                "A new match requires superuser approval.",
                NotificationType.MatchPendingApproval,
                payload));
        }

        await _dispatcher.DispatchAsync(requests, cancellationToken);
    }

    public async Task HandleApprovedAsync(
        MatchApprovedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var (cargoOwnerId, vesselOwnerId) = await _recipientResolver.ResolveMatchOwnersAsync(
            domainEvent.CargoListingId,
            domainEvent.VesselId,
            cancellationToken);

        var superusers = await _recipientResolver.ResolveSuperuserIdsAsync(cancellationToken);
        var payload = NotificationPayloadBuilder.Build(domainEvent);

        var requests = new List<CreateNotificationRequest>
        {
            BuildRequest(
                cargoOwnerId,
                "Match approved",
                "Your cargo match has been approved.",
                NotificationType.MatchApproved,
                payload),
        };

        if (vesselOwnerId.HasValue)
        {
            requests.Add(BuildRequest(
                vesselOwnerId.Value,
                "Match approved",
                "Your vessel match has been approved.",
                NotificationType.MatchApproved,
                payload));
        }

        foreach (var superuserId in superusers)
        {
            requests.Add(BuildRequest(
                superuserId,
                "Match approved",
                "A cargo-vessel match was approved.",
                NotificationType.MatchApproved,
                payload));
        }

        await _dispatcher.DispatchAsync(requests, cancellationToken);
    }

    public async Task HandleRejectedAsync(
        MatchRejectedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var (cargoOwnerId, vesselOwnerId) = await _recipientResolver.ResolveMatchOwnersAsync(
            domainEvent.CargoListingId,
            domainEvent.VesselId,
            cancellationToken);

        var superusers = await _recipientResolver.ResolveSuperuserIdsAsync(cancellationToken);
        var payload = NotificationPayloadBuilder.Build(domainEvent);

        var requests = new List<CreateNotificationRequest>
        {
            BuildRequest(
                cargoOwnerId,
                "Match rejected",
                "A proposed cargo match was rejected.",
                NotificationType.MatchRejected,
                payload),
        };

        if (vesselOwnerId.HasValue)
        {
            requests.Add(BuildRequest(
                vesselOwnerId.Value,
                "Match rejected",
                "A proposed vessel match was rejected.",
                NotificationType.MatchRejected,
                payload));
        }

        foreach (var superuserId in superusers)
        {
            requests.Add(BuildRequest(
                superuserId,
                "Match rejected",
                "A match was rejected.",
                NotificationType.MatchRejected,
                payload));
        }

        await _dispatcher.DispatchAsync(requests, cancellationToken);
    }

    public async Task HandleCancelledAsync(
        MatchCancelledEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var (cargoOwnerId, vesselOwnerId) = await _recipientResolver.ResolveMatchOwnersAsync(
            domainEvent.CargoListingId,
            domainEvent.VesselId,
            cancellationToken);

        var superusers = await _recipientResolver.ResolveSuperuserIdsAsync(cancellationToken);
        var payload = NotificationPayloadBuilder.Build(domainEvent);

        var requests = new List<CreateNotificationRequest>
        {
            BuildRequest(
                cargoOwnerId,
                "Match cancelled",
                "An approved cargo match was cancelled.",
                NotificationType.MatchCancelled,
                payload),
        };

        if (vesselOwnerId.HasValue)
        {
            requests.Add(BuildRequest(
                vesselOwnerId.Value,
                "Match cancelled",
                "An approved vessel match was cancelled.",
                NotificationType.MatchCancelled,
                payload));
        }

        foreach (var superuserId in superusers)
        {
            requests.Add(BuildRequest(
                superuserId,
                "Match cancelled",
                "An approved match was cancelled.",
                NotificationType.MatchCancelled,
                payload));
        }

        await _dispatcher.DispatchAsync(requests, cancellationToken);
    }

    public async Task HandleCompletedAsync(
        MatchCompletedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var (cargoOwnerId, vesselOwnerId) = await _recipientResolver.ResolveMatchOwnersAsync(
            domainEvent.CargoListingId,
            domainEvent.VesselId,
            cancellationToken);

        var superusers = await _recipientResolver.ResolveSuperuserIdsAsync(cancellationToken);
        var payload = NotificationPayloadBuilder.Build(domainEvent);

        var requests = new List<CreateNotificationRequest>
        {
            BuildRequest(
                cargoOwnerId,
                "Match completed",
                "Your cargo shipment has been completed.",
                NotificationType.MatchCompleted,
                payload),
        };

        if (vesselOwnerId.HasValue)
        {
            requests.Add(BuildRequest(
                vesselOwnerId.Value,
                "Match completed",
                "Your vessel assignment has been completed.",
                NotificationType.MatchCompleted,
                payload));
        }

        foreach (var superuserId in superusers)
        {
            requests.Add(BuildRequest(
                superuserId,
                "Match completed",
                "A match was marked as completed.",
                NotificationType.MatchCompleted,
                payload));
        }

        await _dispatcher.DispatchAsync(requests, cancellationToken);
    }

    private static CreateNotificationRequest BuildRequest(
        Guid userId,
        string title,
        string message,
        string notificationType,
        string payload) =>
        new(userId, title, message, notificationType, payload);
}
