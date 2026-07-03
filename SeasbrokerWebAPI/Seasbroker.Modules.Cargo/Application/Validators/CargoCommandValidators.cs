using FluentValidation;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.Constants;

namespace Seasbroker.Modules.Cargo.Application.Validators;

public class CreateCargoListingCommandValidator : AbstractValidator<CreateCargoListingCommand>
{
    public CreateCargoListingCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.CargoType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Weight)
            .GreaterThan(0);

        RuleFor(x => x.Dimensions)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DeparturePort)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.ArrivalPort)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.DepartureTime)
            .LessThan(x => x.ArrivalTime)
            .WithMessage("DepartureTime must be before ArrivalTime.");

        RuleFor(x => x.Status)
            .Must(status => status is null ||
                            status is CargoStatus.Draft or CargoStatus.Open)
            .WithMessage("Status must be Draft or Open when creating a cargo listing.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(CargoConstants.MinPriority, CargoConstants.MaxPriority)
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.ReferenceNumber));

        RuleFor(x => x.AdditionalInfo)
            .MaximumLength(2000)
            .When(x => x.AdditionalInfo is not null);
    }
}

public class UpdateCargoListingCommandValidator : AbstractValidator<UpdateCargoListingCommand>
{
    public UpdateCargoListingCommandValidator()
    {
        RuleFor(x => x.CargoListingId)
            .NotEmpty();

        RuleFor(x => x.CargoType)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.CargoType is not null);

        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Dimensions)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Dimensions is not null);

        RuleFor(x => x.DeparturePort)
            .MinimumLength(2)
            .MaximumLength(200)
            .When(x => x.DeparturePort is not null);

        RuleFor(x => x.ArrivalPort)
            .MinimumLength(2)
            .MaximumLength(200)
            .When(x => x.ArrivalPort is not null);

        RuleFor(x => x.Priority)
            .InclusiveBetween(CargoConstants.MinPriority, CargoConstants.MaxPriority)
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.AdditionalInfo)
            .MaximumLength(2000)
            .When(x => x.AdditionalInfo is not null);
    }
}

public class PromoteQuoteToCargoCommandValidator : AbstractValidator<PromoteQuoteToCargoCommand>
{
    public PromoteQuoteToCargoCommandValidator()
    {
        RuleFor(x => x.RequestedQuoteId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .Must(status => status is null ||
                            status is CargoStatus.Draft or CargoStatus.Open)
            .WithMessage("Status must be Draft or Open when promoting a quote.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(CargoConstants.MinPriority, CargoConstants.MaxPriority)
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.ReferenceNumber));
    }
}
