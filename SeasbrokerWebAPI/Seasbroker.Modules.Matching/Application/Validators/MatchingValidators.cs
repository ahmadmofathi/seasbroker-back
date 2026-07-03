using FluentValidation;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Matching.Application.Commands;
using Seasbroker.Modules.Matching.Application.Exceptions;

namespace Seasbroker.Modules.Matching.Application.Validators;

public class RunMatchingCommandValidator : AbstractValidator<RunMatchingCommand>
{
    public RunMatchingCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.CargoListingId) || string.IsNullOrWhiteSpace(x.VesselId))
            .WithMessage("Specify either cargoListingId or vesselId, not both.");
    }
}

public class ExpireMatchCommandValidator : AbstractValidator<ExpireMatchCommand>
{
    public ExpireMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
    }
}

public class CancelMatchCommandValidator : AbstractValidator<CancelMatchCommand>
{
    public CancelMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
    }
}

public class RunMatchingForCargoCommandValidator : AbstractValidator<RunMatchingForCargoCommand>
{
    public RunMatchingForCargoCommandValidator()
    {
        RuleFor(x => x.CargoListingId).NotEmpty();
    }
}

public class RunMatchingForVesselCommandValidator : AbstractValidator<RunMatchingForVesselCommand>
{
    public RunMatchingForVesselCommandValidator()
    {
        RuleFor(x => x.VesselId).NotEmpty();
    }
}

public class CreateManualMatchCommandValidator : AbstractValidator<CreateManualMatchCommand>
{
    public CreateManualMatchCommandValidator()
    {
        RuleFor(x => x.CargoListingId).NotEmpty();
        RuleFor(x => x.VesselId).NotEmpty();
        RuleFor(x => x.Score)
            .InclusiveBetween(0m, 100m)
            .When(x => x.Score.HasValue);
    }
}

public class UpdateMatchingRuleCommandValidator : AbstractValidator<UpdateMatchingRuleCommand>
{
    public UpdateMatchingRuleCommandValidator()
    {
        RuleFor(x => x.RuleId).NotEmpty();
        RuleFor(x => x.Weight)
            .GreaterThan(0m)
            .When(x => x.Weight.HasValue);
    }
}

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

        throw new MatchingException(result.Errors.First().ErrorMessage, StatusCodes.Status400BadRequest);
    }
}
