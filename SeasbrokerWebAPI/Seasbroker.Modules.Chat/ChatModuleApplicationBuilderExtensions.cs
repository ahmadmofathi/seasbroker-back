using Microsoft.AspNetCore.Builder;
using Seasbroker.Modules.Chat.Hubs;

namespace Seasbroker.Modules.Chat;

public static class ChatModuleApplicationBuilderExtensions
{
    public static WebApplication UseChatModule(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }

    public static WebApplication MapChatModule(this WebApplication app)
    {
        app.MapHub<ChatHub>("/hubs/chat");
        return app;
    }
}
