using FluentValidation;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Cargo.Application.Exceptions;

namespace Seasbroker.Modules.Cargo.Application.Validators;

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
        throw new CargoException(message, StatusCodes.Status400BadRequest);
    }
}
