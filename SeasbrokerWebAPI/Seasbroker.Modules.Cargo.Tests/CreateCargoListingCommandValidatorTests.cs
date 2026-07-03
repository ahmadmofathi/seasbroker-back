using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.Validators;

namespace Seasbroker.Modules.Cargo.Tests;

public class CreateCargoListingCommandValidatorTests
{
    private readonly CreateCargoListingCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_WeightIsZero()
    {
        var command = CreateValidCommand() with { Weight = 0 };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCargoListingCommand.Weight));
    }

    [Fact]
    public void Should_Fail_When_DepartureTimeIsNotBeforeArrivalTime()
    {
        var now = DateTime.UtcNow;
        var command = CreateValidCommand() with { DepartureTime = now, ArrivalTime = now.AddHours(-1) };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Fail_When_StatusIsMatchedOnCreate()
    {
        var command = CreateValidCommand() with { Status = "Matched" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    private static CreateCargoListingCommand CreateValidCommand() =>
        new(
            CustomerId: Guid.NewGuid().ToString(),
            RequestedQuoteId: null,
            ReferenceNumber: null,
            CargoType: "Bulk",
            Weight: 1000,
            Dimensions: "10x10x10",
            DeparturePort: "Rotterdam",
            DepartureTime: DateTime.UtcNow.AddDays(1),
            ArrivalPort: "Singapore",
            ArrivalTime: DateTime.UtcNow.AddDays(20),
            AdditionalInfo: null,
            Status: "Open",
            Priority: 3);
}
