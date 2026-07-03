using FluentValidation;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Approval.Application.Commands;
using Seasbroker.Modules.Approval.Application.Exceptions;

namespace Seasbroker.Modules.Approval.Application.Validators;

public class ApproveMatchCommandValidator : AbstractValidator<ApproveMatchCommand>
{
    public ApproveMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.ApprovedBy).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(2000).When(x => x.Reason is not null);
    }
}

public class RejectMatchCommandValidator : AbstractValidator<RejectMatchCommand>
{
    public RejectMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.RejectedBy).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(2000).When(x => x.Reason is not null);
    }
}

public class CancelMatchCommandValidator : AbstractValidator<CancelMatchCommand>
{
    public CancelMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.CancelledBy).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(2000).When(x => x.Reason is not null);
    }
}

public class CompleteMatchCommandValidator : AbstractValidator<CompleteMatchCommand>
{
    public CompleteMatchCommandValidator()
    {
        RuleFor(x => x.MatchId).NotEmpty();
        RuleFor(x => x.CompletedBy).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(2000).When(x => x.Reason is not null);
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

        throw new ApprovalException(result.Errors.First().ErrorMessage, StatusCodes.Status400BadRequest);
    }
}
