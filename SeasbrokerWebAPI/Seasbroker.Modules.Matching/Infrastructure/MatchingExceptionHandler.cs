using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Exceptions;

namespace Seasbroker.Modules.Matching.Infrastructure;

public class MatchingExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not MatchingException matchingException)
        {
            return false;
        }

        var response = new PocketBaseErrorResponse
        {
            Message = matchingException.Message,
            Status = matchingException.StatusCode,
            Data = string.IsNullOrWhiteSpace(matchingException.Details)
                ? new { }
                : new { details = matchingException.Details },
        };

        httpContext.Response.StatusCode = matchingException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
