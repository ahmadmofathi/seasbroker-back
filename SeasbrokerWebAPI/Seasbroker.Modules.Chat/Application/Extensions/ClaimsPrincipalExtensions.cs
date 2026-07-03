using System.Security.Claims;
using Seasbroker.Modules.Chat.Application.Constants;

namespace Seasbroker.Modules.Chat.Application.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool IsSuperuser(this ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated == true &&
               principal.IsInRole(ChatConstants.SuperuserRole);
    }
}
