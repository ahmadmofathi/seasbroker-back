using FluentValidation;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Constants;

namespace Seasbroker.Modules.Vessel.Application.Validators;

public class UpdateVesselAvailabilityCommandValidator : AbstractValidator<UpdateVesselAvailabilityCommand>
{
    public UpdateVesselAvailabilityCommandValidator()
    {
        RuleFor(x => x.AvailabilityId)
            .NotEmpty();

        RuleFor(x => x.OpenPort)
            .MinimumLength(2)
            .MaximumLength(200)
            .When(x => x.OpenPort is not null);

        RuleFor(x => x.DestinationPort)
            .MaximumLength(200)
            .When(x => x.DestinationPort is not null);
    }
}
