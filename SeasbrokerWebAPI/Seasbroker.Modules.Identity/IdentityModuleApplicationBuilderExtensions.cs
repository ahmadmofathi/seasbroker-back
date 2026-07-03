using Microsoft.AspNetCore.Builder;

namespace Seasbroker.Modules.Identity;

public static class IdentityModuleApplicationBuilderExtensions
{
    public static WebApplication UseIdentityModule(this WebApplication app)
    {
        app.UseAuthentication();
        return app;
    }
}
