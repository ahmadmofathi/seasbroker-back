using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Notifications.Application.DTOs;
using Seasbroker.Modules.Notifications.Application.Exceptions;

namespace Seasbroker.Modules.Notifications.Infrastructure;

public class NotificationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotificationException notificationException)
        {
            return false;
        }

        var response = new PocketBaseErrorResponse
        {
            Message = notificationException.Message,
            Status = notificationException.StatusCode,
            Data = string.IsNullOrWhiteSpace(notificationException.Details)
                ? new { }
                : new { details = notificationException.Details },
        };

        httpContext.Response.StatusCode = notificationException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
