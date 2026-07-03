using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Notifications.Application.Abstractions;
using Seasbroker.Modules.Notifications.Application.Constants;
using Seasbroker.Modules.Notifications.Application.DTOs;
using Seasbroker.Modules.Notifications.Application.Queries;
using Seasbroker.Modules.Notifications.Application.Services;

namespace Seasbroker.Modules.Notifications.Controllers;

/// <summary>
/// User notification inbox API.
/// </summary>
[ApiController]
[Authorize(Policy = NotificationConstants.SuperuserPolicy)]
[Tags("Notifications")]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public NotificationsController(
        INotificationService notificationService,
        ICurrentUserAccessor currentUserAccessor)
    {
        _notificationService = notificationService;
        _currentUserAccessor = currentUserAccessor;
    }

    /// <summary>
    /// Lists notifications for the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(NotificationListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserAccessor.GetRequiredUserId();
        var result = await _notificationService.GetForUserAsync(
            new GetNotificationsQuery(userId, page, perPage),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lists unread notifications for the authenticated user.
    /// </summary>
    [HttpGet("unread")]
    [ProducesResponseType(typeof(NotificationListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Unread(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserAccessor.GetRequiredUserId();
        var result = await _notificationService.GetUnreadForUserAsync(
            new GetUnreadNotificationsQuery(userId, page, perPage),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    [HttpPost("{id}/read")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var notificationId))
        {
            return NotFound();
        }

        var userId = _currentUserAccessor.GetRequiredUserId();
        var result = await _notificationService.MarkReadAsync(
            new MarkNotificationReadCommand(userId, notificationId),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Marks all notifications as read for the authenticated user.
    /// </summary>
    [HttpPost("read-all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.GetRequiredUserId();
        var updatedCount = await _notificationService.MarkAllReadAsync(
            new MarkAllNotificationsReadCommand(userId),
            cancellationToken);

        return Ok(new { updated = updatedCount });
    }

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var notificationId))
        {
            return NotFound();
        }

        var userId = _currentUserAccessor.GetRequiredUserId();
        await _notificationService.DeleteAsync(
            new DeleteNotificationCommand(userId, notificationId),
            cancellationToken);

        return NoContent();
    }
}
