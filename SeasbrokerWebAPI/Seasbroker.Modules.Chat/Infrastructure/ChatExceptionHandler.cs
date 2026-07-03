using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Application.Exceptions;

namespace Seasbroker.Modules.Chat.Infrastructure;

public class ChatExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ChatException chatException)
        {
            return false;
        }

        var response = new PocketBaseErrorResponse
        {
            Message = chatException.Message,
            Status = chatException.StatusCode,
            Data = string.IsNullOrWhiteSpace(chatException.Details)
                ? new { }
                : new { details = chatException.Details },
        };

        httpContext.Response.StatusCode = chatException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
