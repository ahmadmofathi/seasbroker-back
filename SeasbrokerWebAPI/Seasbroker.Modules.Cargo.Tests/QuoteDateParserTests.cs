using Seasbroker.Modules.Cargo.Application.Helpers;

namespace Seasbroker.Modules.Cargo.Tests;

public class QuoteDateParserTests
{
    [Fact]
    public void ParseOrThrow_ParsesIsoDate()
    {
        var parsed = QuoteDateParser.ParseOrThrow("2026-07-01T10:00:00Z", "departureTime");

        Assert.Equal(2026, parsed.Year);
        Assert.Equal(7, parsed.Month);
    }

    [Fact]
    public void ParseOrThrow_Throws_WhenValueIsInvalid()
    {
        Assert.Throws<Application.Exceptions.CargoException>(() =>
            QuoteDateParser.ParseOrThrow("not-a-date", "departureTime"));
    }
}
