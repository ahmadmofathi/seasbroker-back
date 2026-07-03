using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Exceptions;

namespace Seasbroker.Modules.Vessel.Infrastructure;

public class VesselExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not VesselException vesselException)
        {
            return false;
        }

        var response = new PocketBaseErrorResponse
        {
            Message = vesselException.Message,
            Status = vesselException.StatusCode,
            Data = string.IsNullOrWhiteSpace(vesselException.Details)
                ? new { }
                : new { details = vesselException.Details },
        };

        httpContext.Response.StatusCode = vesselException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
