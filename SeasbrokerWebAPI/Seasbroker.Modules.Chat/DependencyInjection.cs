using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Seasbroker.Modules.Chat.Application.Abstractions;
using Seasbroker.Modules.Chat.Application.Services;
using Seasbroker.Modules.Chat.Hubs;
using Seasbroker.Modules.Chat.Infrastructure;
using Seasbroker.Modules.Chat.Infrastructure.Options;

namespace Seasbroker.Modules.Chat;

public static class DependencyInjection
{
    public static IServiceCollection AddChatModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeoLocationOptions>(configuration.GetSection(GeoLocationOptions.SectionName));

        services.AddHttpClient<IGeoLocationService, GeoLocationService>();

        services.AddSignalR();
        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerSignalRPostConfigure>();
        services.AddScoped<IChatNotificationService, ChatNotificationService>();

        services.AddScoped<IChatTokenService, ChatTokenService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IMessageService, MessageService>();

        services.AddExceptionHandler<ChatExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IMvcBuilder AddChatModuleControllers(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);
    }
}
