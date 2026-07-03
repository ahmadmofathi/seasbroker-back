using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Approval.Application.Abstractions;
using Seasbroker.Modules.Approval.Application.Exceptions;

namespace Seasbroker.Modules.Approval.Infrastructure;

public class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetRequiredUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new ApprovalException("Authentication is required.", StatusCodes.Status401Unauthorized);
        }

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new ApprovalException("Authenticated user identifier is invalid.", StatusCodes.Status401Unauthorized);
        }

        return userId;
    }
}
