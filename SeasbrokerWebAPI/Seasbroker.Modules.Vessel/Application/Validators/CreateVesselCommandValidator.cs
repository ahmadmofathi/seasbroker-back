using FluentValidation;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Constants;
using Seasbroker.Infrastructure.Persistence.Entities;

namespace Seasbroker.Modules.Vessel.Application.Validators;

public class CreateVesselCommandValidator : AbstractValidator<CreateVesselCommand>
{
    public CreateVesselCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.ImoNumber)
            .Matches(@"^\d{7}$")
            .When(x => !string.IsNullOrWhiteSpace(x.ImoNumber))
            .WithMessage("IMO number must be exactly 7 digits.");

        RuleFor(x => x.VesselType)
            .NotEmpty()
            .Must(type => VesselConstants.AllowedVesselTypes.Contains(type))
            .WithMessage("Vessel type is not supported.");

        RuleFor(x => x.Dwt)
            .GreaterThan(0);

        RuleFor(x => x.CurrentPort)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

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
