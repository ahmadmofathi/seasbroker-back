using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Quote.Application.DTOs;
using Seasbroker.Modules.Quote.Application.Exceptions;

namespace Seasbroker.Modules.Quote.Infrastructure;

public class QuoteExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not QuoteException quoteException)
        {
            return false;
        }

        var response = new PocketBaseErrorResponse
        {
            Message = quoteException.Message,
            Status = quoteException.StatusCode,
            Data = string.IsNullOrWhiteSpace(quoteException.Details)
                ? new { }
                : new { details = quoteException.Details },
        };

        httpContext.Response.StatusCode = quoteException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
