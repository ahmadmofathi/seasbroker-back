using FluentValidation;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Constants;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Modules.Vessel.Application.Validators;

public class UpdateVesselCommandValidator : AbstractValidator<UpdateVesselCommand>
{
    public UpdateVesselCommandValidator()
    {
        RuleFor(x => x.VesselId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .MinimumLength(2)
            .MaximumLength(200)
            .When(x => x.Name is not null);

        RuleFor(x => x.VesselType)
            .Must(type => type is null || VesselConstants.AllowedVesselTypes.Contains(type))
            .WithMessage("Vessel type is not supported.");

        RuleFor(x => x.Dwt)
            .GreaterThan(0)
            .When(x => x.Dwt.HasValue);

        RuleFor(x => x.CurrentPort)
            .MinimumLength(2)
            .MaximumLength(200)
            .When(x => x.CurrentPort is not null);

        RuleFor(x => x.FlagCountry)
            .MaximumLength(100)
            .When(x => x.FlagCountry is not null);

        RuleFor(x => x.Status)
            .Must(status => status is null ||
                            status is VesselStatus.Active or VesselStatus.Inactive or VesselStatus.Maintenance)
            .WithMessage("Invalid vessel status.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
