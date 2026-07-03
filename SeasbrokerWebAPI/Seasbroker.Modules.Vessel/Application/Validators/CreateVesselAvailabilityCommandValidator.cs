using FluentValidation;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Constants;

namespace Seasbroker.Modules.Vessel.Application.Validators;

public class CreateVesselAvailabilityCommandValidator : AbstractValidator<CreateVesselAvailabilityCommand>
{
    public CreateVesselAvailabilityCommandValidator()
    {
        RuleFor(x => x.VesselId)
            .NotEmpty();

        RuleFor(x => x.AvailableFrom)
            .LessThan(x => x.AvailableTo)
            .WithMessage("AvailableFrom must be before AvailableTo.");

        RuleFor(x => x)
            .Must(x => (x.AvailableTo - x.AvailableFrom).TotalDays <= VesselConstants.MaxAvailabilityWindowDays)
            .WithMessage($"Availability window cannot exceed {VesselConstants.MaxAvailabilityWindowDays} days.");

        RuleFor(x => x.OpenPort)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.DestinationPort)
            .MaximumLength(200)
            .When(x => x.DestinationPort is not null);
    }
}
