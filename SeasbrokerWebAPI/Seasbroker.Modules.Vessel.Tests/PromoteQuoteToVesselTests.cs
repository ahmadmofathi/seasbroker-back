using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Exceptions;
using Seasbroker.Modules.Vessel.Application.Handlers.Commands;

namespace Seasbroker.Modules.Vessel.Tests;

public class PromoteQuoteToVesselTests
{
    private static async Task<(SeasbrokerDbContext Db, RequestedQuote Quote)> SeedAsync(
        string formKey,
        Dictionary<string, string> values,
        bool withWrongCargoListing = false)
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new SeasbrokerDbContext(options);

        var customer = new Customer { Email = "owner@test.com", PhoneNumber = "1", FirstName = "Blue", LastName = "Wave" };
        var quote = new RequestedQuote
        {
            CustomerId = customer.Id,
            CargoType = values.GetValueOrDefault("vesselType", "Bulk Carrier"),
            Weight = 6665,
            DeparturePort = "Abu Dhabi - United Arab Emirates",
            AdditionalInfo = "[Ship Brokerage] details",
        };
        var definition = new FormDefinition { Key = formKey, Name = formKey };
        var version = new FormVersion { FormDefinitionId = definition.Id, VersionNumber = 1, Status = FormVersionStatus.Published };
        var submission = new FormSubmission { FormVersionId = version.Id, CustomerId = customer.Id, RequestedQuoteId = quote.Id };
        foreach (var (key, value) in values)
        {
            submission.Values.Add(new FormSubmissionValue { FormSubmissionId = submission.Id, FieldKey = key, ValueText = value });
        }

        db.AddRange(customer, quote, definition, version, submission);
        if (withWrongCargoListing)
        {
            db.CargoListings.Add(new CargoListing
            {
                CustomerId = customer.Id,
                RequestedQuoteId = quote.Id,
                ReferenceNumber = "CRG-WRONG",
                CargoType = "Bulk Carrier",
                Status = CargoStatus.Open,
                DepartureTime = DateTime.UtcNow,
                ArrivalTime = DateTime.UtcNow.AddDays(1),
            });
        }

        await db.SaveChangesAsync();
        return (db, quote);
    }

    private static Dictionary<string, string> OpenVessel() => new()
    {
        ["vesselName"] = "hamdy ahmed",
        ["imoNumber"] = "1701066",
        ["vesselType"] = "Bulk Carrier",
        ["dwt"] = "6665",
        ["flag"] = "Greece",
        ["currentOpenPort"] = "Abu Dhabi - United Arab Emirates",
        ["openDateTo"] = "2026-10-20",
        ["availabilityType"] = "Open Vessel",
        ["openPort"] = "Abu Dhabi - United Arab Emirates",
        ["openDate"] = "2026-10-02",
        ["preferredDestinationArea"] = "Al Fujayrah - United Arab Emirates",
    };

    [Fact]
    public async Task OpenVessel_Request_Becomes_A_Vessel_With_An_Availability_Window()
    {
        var (db, quote) = await SeedAsync(FormDefinition.ShipRequestKey, OpenVessel());
        await using var _ = db;

        var result = await new PromoteQuoteToVesselCommandHandler(db).HandleAsync(new PromoteQuoteToVesselCommand(quote.Id.ToString()));

        Assert.Equal("hamdy ahmed", result.Vessel.Name);
        Assert.Equal("Bulk", result.Vessel.VesselType);
        Assert.Equal(6665, result.Vessel.Dwt);
        Assert.Equal(quote.Id.ToString(), result.Vessel.RequestedQuote);

        Assert.NotNull(result.Availability);
        Assert.Equal(new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc), result.Availability.AvailableFrom);
        Assert.Equal(new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc), result.Availability.AvailableTo);
        Assert.Equal("Abu Dhabi - United Arab Emirates", result.Availability.OpenPort);
        Assert.Equal("Al Fujayrah - United Arab Emirates", result.Availability.DestinationPort);
    }

    [Fact]
    public async Task ScheduledRoute_Request_Keeps_The_Route_In_Order()
    {
        var values = OpenVessel();
        values["availabilityType"] = "Scheduled Route";
        values["schedRoute"] = """[{"port":"Jeddah - Saudi Arabia","eta":"2026-10-05"},{"port":"Port Said - Egypt","eta":"2026-10-09"},{"port":"Piraeus - Greece","eta":"2026-10-13"}]""";
        var (db, quote) = await SeedAsync(FormDefinition.ShipRequestKey, values);
        await using var _ = db;

        var result = await new PromoteQuoteToVesselCommandHandler(db).HandleAsync(new PromoteQuoteToVesselCommand(quote.Id.ToString()));

        Assert.NotNull(result.Availability);
        Assert.Equal(new[] { "Jeddah - Saudi Arabia", "Port Said - Egypt", "Piraeus - Greece" }, result.Availability.RouteStops.Select(s => s.Port));
        Assert.Equal("Piraeus - Greece", result.Availability.DestinationPort);
    }

    [Fact]
    public async Task Cancels_A_Cargo_Listing_That_Was_Made_From_The_Ship_Request_By_Mistake()
    {
        var (db, quote) = await SeedAsync(FormDefinition.ShipRequestKey, OpenVessel(), withWrongCargoListing: true);
        await using var _ = db;

        var result = await new PromoteQuoteToVesselCommandHandler(db).HandleAsync(new PromoteQuoteToVesselCommand(quote.Id.ToString()));

        Assert.Equal("CRG-WRONG", result.CancelledCargoListingReference);
        var listing = await db.CargoListings.SingleAsync();
        Assert.Equal(CargoStatus.Cancelled, listing.Status);
        Assert.Null(listing.RequestedQuoteId);
    }

    [Fact]
    public async Task Rejects_Requests_That_Are_Not_Ship_Brokerage()
    {
        var (db, quote) = await SeedAsync(FormDefinition.CargoRequestKey, OpenVessel());
        await using var _ = db;

        await Assert.ThrowsAsync<VesselException>(() =>
            new PromoteQuoteToVesselCommandHandler(db).HandleAsync(new PromoteQuoteToVesselCommand(quote.Id.ToString())));
        Assert.Empty(db.Vessels);
    }

    [Fact]
    public async Task Rejects_Promoting_The_Same_Request_Twice()
    {
        var (db, quote) = await SeedAsync(FormDefinition.ShipRequestKey, OpenVessel());
        await using var _ = db;
        var handler = new PromoteQuoteToVesselCommandHandler(db);
        await handler.HandleAsync(new PromoteQuoteToVesselCommand(quote.Id.ToString()));

        await Assert.ThrowsAsync<VesselException>(() => handler.HandleAsync(new PromoteQuoteToVesselCommand(quote.Id.ToString())));
        Assert.Single(db.Vessels);
    }
}
