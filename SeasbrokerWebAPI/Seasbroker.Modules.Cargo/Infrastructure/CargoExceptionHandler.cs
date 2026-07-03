using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Exceptions;

namespace Seasbroker.Modules.Cargo.Infrastructure;

public class CargoExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not CargoException cargoException)
        {
            return false;
        }

        var response = new PocketBaseErrorResponse
        {
            Message = cargoException.Message,
            Status = cargoException.StatusCode,
            Data = string.IsNullOrWhiteSpace(cargoException.Details)
                ? new { }
                : new { details = cargoException.Details },
        };

        httpContext.Response.StatusCode = cargoException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
