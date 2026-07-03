using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Approval.Application.DTOs;
using Seasbroker.Modules.Approval.Application.Exceptions;

namespace Seasbroker.Modules.Approval.Infrastructure;

public class ApprovalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ApprovalException approvalException)
        {
            return false;
        }

        var response = new PocketBaseErrorResponse
        {
            Message = approvalException.Message,
            Status = approvalException.StatusCode,
            Data = string.IsNullOrWhiteSpace(approvalException.Details)
                ? new { }
                : new { details = approvalException.Details },
        };

        httpContext.Response.StatusCode = approvalException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
