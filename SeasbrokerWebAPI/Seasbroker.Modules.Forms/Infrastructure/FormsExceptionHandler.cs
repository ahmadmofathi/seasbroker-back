using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Exceptions;

namespace Seasbroker.Modules.Forms.Infrastructure;

public class FormsExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not FormsException formsException)
        {
            return false;
        }

        var response = new PocketBaseErrorResponse
        {
            Message = formsException.Message,
            Status = formsException.StatusCode,
            Data = string.IsNullOrWhiteSpace(formsException.Details)
                ? new { }
                : new { details = formsException.Details },
        };

        httpContext.Response.StatusCode = formsException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
