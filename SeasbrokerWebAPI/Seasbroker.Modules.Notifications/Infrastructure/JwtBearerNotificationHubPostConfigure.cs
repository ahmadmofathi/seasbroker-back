using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Seasbroker.Modules.Notifications.Infrastructure;

public class JwtBearerNotificationHubPostConfigure : IPostConfigureOptions<JwtBearerOptions>
{
    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        var previousOnMessageReceived = options.Events.OnMessageReceived;

        options.Events.OnMessageReceived = async context =>
        {
            if (previousOnMessageReceived is not null)
            {
                await previousOnMessageReceived(context);
            }

            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
        };
    }
}
