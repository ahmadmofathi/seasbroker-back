using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Validators;

namespace Seasbroker.Modules.Vessel.Tests;

public class CreateVesselCommandValidatorTests
{
    private readonly CreateVesselCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_ImoNumberIsNotSevenDigits()
    {
        var command = CreateValidCommand() with { ImoNumber = "12345" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateVesselCommand.ImoNumber));
    }

    [Fact]
    public void Should_Pass_When_ImoNumberIsSevenDigits()
    {
        var command = CreateValidCommand() with { ImoNumber = "1234567" };

        var result = _validator.Validate(command);

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(CreateVesselCommand.ImoNumber));
    }

    [Fact]
    public void Should_Fail_When_VesselTypeIsNotAllowed()
    {
        var command = CreateValidCommand() with { VesselType = "Submarine" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateVesselCommand.VesselType));
    }

    [Fact]
    public void Should_Fail_When_DwtIsZeroOrNegative()
    {
        var command = CreateValidCommand() with { Dwt = 0 };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateVesselCommand.Dwt));
    }

    private static CreateVesselCommand CreateValidCommand() =>
        new(
            Name: "MV Seasbroker",
            ImoNumber: "7654321",
            VesselType: "Bulk",
            Dwt: 50000,
            TeuCapacity: null,
            LengthOverall: 200,
            Beam: 32,
            Draft: 12,
            CurrentPort: "Rotterdam",
            FlagCountry: "NL",
            Status: null,
            CustomerId: null,
            Notes: null);
}
