using FluentValidation;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Vessel.Application.Exceptions;

namespace Seasbroker.Modules.Vessel.Application.Validators;

public static class ValidationExtensions
{
    public static async Task ValidateCommandAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);

        if (result.IsValid)
        {
            return;
        }

        var message = result.Errors.First().ErrorMessage;
        throw new VesselException(message, StatusCodes.Status400BadRequest);
    }
}
