using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Seasbroker.Modules.Notifications.Application.Constants;

namespace Seasbroker.Modules.Notifications.Hubs;

public class NotificationHub : Hub
{
    [Authorize]
    public async Task JoinUser()
    {
        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new HubException("Invalid user subscription.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, NotificationHubGroups.ForUser(userId));
    }

    [Authorize(Policy = NotificationConstants.SuperuserPolicy)]
    public async Task JoinAdmin()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, NotificationHubGroups.Admin);
    }
}
