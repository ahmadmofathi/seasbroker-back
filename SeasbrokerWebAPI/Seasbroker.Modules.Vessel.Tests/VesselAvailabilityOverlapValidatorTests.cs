using Seasbroker.Modules.Vessel.Application.Helpers;

namespace Seasbroker.Modules.Vessel.Tests;

public class VesselAvailabilityOverlapValidatorTests
{
    [Fact]
    public void HasOverlap_ReturnsTrue_WhenWindowsOverlap()
    {
        var existing = new[] { (From: new DateTime(2026, 1, 10), To: new DateTime(2026, 1, 20)) };

        var result = VesselAvailabilityOverlapValidator.HasOverlap(
            new DateTime(2026, 1, 15),
            new DateTime(2026, 1, 25),
            existing);

        Assert.True(result);
    }

    [Fact]
    public void HasOverlap_ReturnsFalse_WhenWindowsAreAdjacent()
    {
        var existing = new[] { (From: new DateTime(2026, 1, 10), To: new DateTime(2026, 1, 20)) };

        var result = VesselAvailabilityOverlapValidator.HasOverlap(
            new DateTime(2026, 1, 20),
            new DateTime(2026, 1, 30),
            existing);

        Assert.False(result);
    }

    [Fact]
    public void EnsureNoOverlap_ExcludesSpecifiedAvailability()
    {
        var existing = new List<(Guid Id, DateTime From, DateTime To)>
        {
            (Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 2, 1), new DateTime(2026, 2, 10)),
        };

        var exception = Record.Exception(() =>
            VesselAvailabilityOverlapValidator.EnsureNoOverlap(
                new DateTime(2026, 2, 1),
                new DateTime(2026, 2, 10),
                existing,
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureNoOverlap_Throws_WhenAnotherWindowOverlaps()
    {
        var existing = new List<(Guid Id, DateTime From, DateTime To)>
        {
            (Guid.NewGuid(), new DateTime(2026, 3, 1), new DateTime(2026, 3, 15)),
        };

        Assert.Throws<InvalidOperationException>(() =>
            VesselAvailabilityOverlapValidator.EnsureNoOverlap(
                new DateTime(2026, 3, 10),
                new DateTime(2026, 3, 20),
                existing));
    }
}
