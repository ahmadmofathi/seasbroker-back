using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Seasbroker.Modules.Approval.Application.Abstractions;
using ApprovalDispatcher = Seasbroker.Modules.Approval.Application.Abstractions.IDomainEventDispatcher;
using Seasbroker.Modules.Chat.Application.Abstractions;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Infrastructure;
using MatchingDispatcher = Seasbroker.Modules.Matching.Application.Abstractions.IDomainEventDispatcher;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Notifications.Application.Abstractions;
using NotificationsCurrentUserAccessor = Seasbroker.Modules.Notifications.Application.Abstractions.ICurrentUserAccessor;
using Seasbroker.Modules.Notifications.Application.EventHandlers;
using Seasbroker.Modules.Notifications.Application.Services;
using Seasbroker.Modules.Notifications.Hubs;
using Seasbroker.Modules.Notifications.Infrastructure;

namespace Seasbroker.Modules.Notifications;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
        services.AddScoped<INotificationRecipientResolver, NotificationRecipientResolver>();
        services.AddScoped<IMatchNotificationEventHandler, MatchNotificationEventHandler>();
        services.AddScoped<NotificationsCurrentUserAccessor, CurrentUserAccessor>();

        services.AddScoped<MatchingDomainEventBridge>();
        services.AddScoped<MatchingDispatcher>(sp => sp.GetRequiredService<MatchingDomainEventBridge>());

        services.AddScoped<ApprovalDomainEventBridge>();
        services.AddScoped<ApprovalDispatcher>(sp => sp.GetRequiredService<ApprovalDomainEventBridge>());

        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerNotificationHubPostConfigure>();

        services.DecorateChatNotificationService();

        services.AddExceptionHandler<NotificationExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IMvcBuilder AddNotificationsModuleControllers(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);
    }

    private static IServiceCollection DecorateChatNotificationService(this IServiceCollection services)
    {
        services.AddScoped<IChatNotificationService>(sp =>
            new ChatNotificationDecorator(
                new ChatNotificationService(sp.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<Seasbroker.Modules.Chat.Hubs.ChatHub>>()),
                sp.GetRequiredService<INotificationDispatcher>(),
                sp.GetRequiredService<INotificationRecipientResolver>()));

        return services;
    }
}

public static class NotificationsModuleApplicationBuilderExtensions
{
    public static WebApplication UseNotificationsModule(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }

    public static WebApplication MapNotificationsModule(this WebApplication app)
    {
        app.MapHub<NotificationHub>("/hubs/notifications");
        return app;
    }
}

internal sealed class ChatNotificationDecorator : IChatNotificationService
{
    private readonly ChatNotificationService _inner;
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _recipientResolver;

    public ChatNotificationDecorator(
        ChatNotificationService inner,
        INotificationDispatcher dispatcher,
        INotificationRecipientResolver recipientResolver)
    {
        _inner = inner;
        _dispatcher = dispatcher;
        _recipientResolver = recipientResolver;
    }

    public async Task NotifyChatCreatedAsync(ChatRecordDto chat, CancellationToken cancellationToken = default)
    {
        await _inner.NotifyChatCreatedAsync(chat, cancellationToken);
    }

    public async Task NotifyMessageCreatedAsync(MessageRecordDto message, CancellationToken cancellationToken = default)
    {
        await _inner.NotifyMessageCreatedAsync(message, cancellationToken);

        var superusers = await _recipientResolver.ResolveSuperuserIdsAsync(cancellationToken);
        if (superusers.Count == 0)
        {
            return;
        }

        var payload = NotificationPayloadBuilder.Build(message);
        var requests = superusers.Select(superuserId => new Application.Queries.CreateNotificationRequest(
            superuserId,
            "New chat message",
            "A new message was posted in chat.",
            NotificationType.NewChatMessage,
            payload)).ToList();

        await _dispatcher.DispatchAsync(requests, cancellationToken);
    }
}
