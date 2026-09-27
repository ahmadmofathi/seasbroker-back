using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.Handlers.Commands;
using Seasbroker.Modules.Cargo.Application.Handlers.Queries;
using Seasbroker.Modules.Cargo.Application.Queries;
using Seasbroker.Modules.Cargo.Application.Validators;

namespace Seasbroker.Modules.Cargo.Tests;

public class CargoBusinessRulesTests
{
    [Fact]
    public async Task Update_Rejects_WhenStatusIsNotOpen()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new SeasbrokerDbContext(options);

        var listing = new CargoListing
        {
            CustomerId = Guid.NewGuid(),
            ReferenceNumber = "CRG-20260701-000001",
            CargoType = "Bulk",
            Weight = 100,
            Dimensions = "1x1x1",
            DeparturePort = "A",
            DepartureTime = DateTime.UtcNow,
            ArrivalPort = "B",
            ArrivalTime = DateTime.UtcNow.AddDays(5),
            Status = CargoStatus.Closed,
        };

        dbContext.CargoListings.Add(listing);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateCargoListingCommandHandler(
            dbContext,
            new UpdateCargoListingCommandValidator());

        await Assert.ThrowsAsync<Application.Exceptions.CargoException>(() =>
            handler.HandleAsync(new UpdateCargoListingCommand(
                listing.Id.ToString(),
                CargoType: "Container",
                Weight: null,
                Dimensions: null,
                DeparturePort: null,
                DepartureTime: null,
                ArrivalPort: null,
                ArrivalTime: null,
                AdditionalInfo: null,
                Priority: null)));
    }

    [Fact]
    public async Task GetOpenCargoForMatching_ReturnsOnlyOpenListings()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new SeasbrokerDbContext(options);

        dbContext.CargoListings.AddRange(
            new CargoListing
            {
                CustomerId = Guid.NewGuid(),
                ReferenceNumber = "CRG-20260701-000001",
                CargoType = "Bulk",
                Weight = 100,
                Dimensions = "1x1x1",
                DeparturePort = "A",
                DepartureTime = DateTime.UtcNow,
                ArrivalPort = "B",
                ArrivalTime = DateTime.UtcNow.AddDays(5),
                Status = CargoStatus.Open,
            },
            new CargoListing
            {
                CustomerId = Guid.NewGuid(),
                ReferenceNumber = "CRG-20260701-000002",
                CargoType = "Bulk",
                Weight = 200,
                Dimensions = "2x2x2",
                DeparturePort = "A",
                DepartureTime = DateTime.UtcNow,
                ArrivalPort = "B",
                ArrivalTime = DateTime.UtcNow.AddDays(5),
                Status = CargoStatus.Closed,
            });

        await dbContext.SaveChangesAsync();

        var handler = new GetOpenCargoForMatchingQueryHandler(dbContext);
        var results = await handler.HandleAsync(new GetOpenCargoForMatchingQuery());

        Assert.Single(results);
        Assert.Equal(CargoStatus.Open, results[0].Status);
    }

    [Fact]
    public async Task PromoteQuote_RejectsDuplicatePromotion()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new SeasbrokerDbContext(options);

        var customerId = Guid.NewGuid();
        dbContext.Customers.Add(new Customer
        {
            Email = "cargo@test.com",
            PhoneNumber = "123",
            FirstName = "Test",
            LastName = "User",
        });

        await dbContext.SaveChangesAsync();

        customerId = dbContext.Customers.Single().Id;

        var quote = new RequestedQuote
        {
            CustomerId = customerId,
            CargoType = "Bulk",
            Weight = 500,
            DeparturePort = "Hamburg",
            DepartureTime = "2026-08-01T00:00:00Z",
            ArrivalPort = "Dubai",
            ArrivalTime = "2026-08-15T00:00:00Z",
            Dimensions = "10x10",
        };

        dbContext.RequestedQuotes.Add(quote);
        await dbContext.SaveChangesAsync();

        dbContext.CargoListings.Add(new CargoListing
        {
            CustomerId = customerId,
            RequestedQuoteId = quote.Id,
            ReferenceNumber = "CRG-20260701-000001",
            CargoType = quote.CargoType,
            Weight = quote.Weight,
            Dimensions = quote.Dimensions,
            DeparturePort = quote.DeparturePort,
            DepartureTime = DateTime.UtcNow,
            ArrivalPort = quote.ArrivalPort,
            ArrivalTime = DateTime.UtcNow.AddDays(10),
            Status = CargoStatus.Open,
        });

        await dbContext.SaveChangesAsync();

        var handler = new PromoteQuoteToCargoCommandHandler(
            dbContext,
            new PromoteQuoteToCargoCommandValidator());

        await Assert.ThrowsAsync<Application.Exceptions.CargoException>(() =>
            handler.HandleAsync(new PromoteQuoteToCargoCommand(quote.Id.ToString(), null, null, null)));
    }

    [Theory]
    [InlineData("Dry Bulk", "request-route")]
    [InlineData("Dry Bulk", "request-clearance")]
    [InlineData(RequestedQuote.ContactInquiryCargoType, null)]
    public async Task PromoteQuote_RejectsRequestsThatAreNotCargo(string cargoType, string? sourceFormKey)
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new SeasbrokerDbContext(options);

        var customer = new Customer { Email = "ship@test.com", PhoneNumber = "123", FirstName = "Test", LastName = "User" };
        dbContext.Customers.Add(customer);

        var quote = new RequestedQuote
        {
            CustomerId = customer.Id,
            CargoType = cargoType,
            Weight = 500,
            DeparturePort = "Hamburg",
            DepartureTime = "2026-08-01T00:00:00Z",
            ArrivalPort = "Dubai",
            ArrivalTime = "2026-08-15T00:00:00Z",
            Dimensions = "10x10",
        };
        dbContext.RequestedQuotes.Add(quote);

        if (sourceFormKey is not null)
        {
            var definition = new FormDefinition { Key = sourceFormKey, Name = sourceFormKey };
            var version = new FormVersion { FormDefinitionId = definition.Id, VersionNumber = 1, Status = FormVersionStatus.Published };
            dbContext.FormDefinitions.Add(definition);
            dbContext.FormVersions.Add(version);
            dbContext.FormSubmissions.Add(new FormSubmission { FormVersionId = version.Id, CustomerId = customer.Id, RequestedQuoteId = quote.Id });
        }

        await dbContext.SaveChangesAsync();

        var handler = new PromoteQuoteToCargoCommandHandler(dbContext, new PromoteQuoteToCargoCommandValidator());

        var ex = await Assert.ThrowsAsync<Application.Exceptions.CargoException>(() =>
            handler.HandleAsync(new PromoteQuoteToCargoCommand(quote.Id.ToString(), null, null, null)));
        Assert.Contains("Only Cargo Brokerage requests", ex.Message);
        Assert.Empty(dbContext.CargoListings);
    }

    [Theory]
    [InlineData(FormDefinition.CargoRequestKey)]
    [InlineData(null)]
    public void IsCargoRequest_AcceptsCargoFormAndDirectQuotes(string? sourceFormKey)
    {
        Assert.True(RequestedQuote.IsCargoRequest("Dry Bulk", sourceFormKey));
    }

    [Fact]
    public async Task PromoteQuote_UsesEditedValues_AndLeavesTheRequestUntouched()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new SeasbrokerDbContext(options);

        var customer = new Customer { Email = "edit@test.com", PhoneNumber = "1", FirstName = "Edit", LastName = "Me" };
        var quote = new RequestedQuote
        {
            CustomerId = customer.Id,
            CargoType = "Dry Bulk",
            Weight = 500,
            DeparturePort = "Hamburg",
            DepartureTime = "2026-11-01T00:00:00Z",
            ArrivalPort = "Dubai",
            ArrivalTime = "2026-11-15T00:00:00Z",
            Dimensions = "10x10",
        };
        dbContext.AddRange(customer, quote);
        await dbContext.SaveChangesAsync();

        var edits = new PromoteQuoteOverrides(
            Weight: 750,
            ArrivalPort: "Jebel Ali - United Arab Emirates",
            ArrivalTime: new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc));
        var listing = await new PromoteQuoteToCargoCommandHandler(dbContext, new PromoteQuoteToCargoCommandValidator())
            .HandleAsync(new PromoteQuoteToCargoCommand(quote.Id.ToString(), null, null, null, edits));

        Assert.Equal(750, listing.Weight);
        Assert.Equal("Jebel Ali - United Arab Emirates", listing.ArrivalPort);
        Assert.Equal("Hamburg", listing.DeparturePort); // not edited, taken from the request
        Assert.Equal("Dry Bulk", listing.CargoType);

        var original = await dbContext.RequestedQuotes.AsNoTracking().SingleAsync();
        Assert.Equal(500, original.Weight);
        Assert.Equal("Dubai", original.ArrivalPort);
    }

    [Fact]
    public async Task PromoteQuote_RejectsEditedArrivalBeforeDeparture()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new SeasbrokerDbContext(options);

        var customer = new Customer { Email = "bad@test.com", PhoneNumber = "1", FirstName = "Bad", LastName = "Dates" };
        var quote = new RequestedQuote
        {
            CustomerId = customer.Id,
            CargoType = "Dry Bulk",
            Weight = 500,
            DeparturePort = "Hamburg",
            DepartureTime = "2026-11-01T00:00:00Z",
            ArrivalPort = "Dubai",
            ArrivalTime = "2026-11-15T00:00:00Z",
            Dimensions = "10x10",
        };
        dbContext.AddRange(customer, quote);
        await dbContext.SaveChangesAsync();

        var edits = new PromoteQuoteOverrides(ArrivalTime: new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        await Assert.ThrowsAsync<Application.Exceptions.CargoException>(() =>
            new PromoteQuoteToCargoCommandHandler(dbContext, new PromoteQuoteToCargoCommandValidator())
                .HandleAsync(new PromoteQuoteToCargoCommand(quote.Id.ToString(), null, null, null, edits)));
        Assert.Empty(dbContext.CargoListings);
    }

    [Theory]
    [InlineData("[Ship Brokerage] Vessel Name: Sea Star", false)]
    [InlineData("[Customs Clearance] Import", false)]
    [InlineData("[Contact] Subject: hi", false)]
    [InlineData("[Cargo Brokerage] Commodity: wheat", true)]
    [InlineData("Old quote with no tag", true)]
    [InlineData(null, true)]
    public void IsCargoRequest_UsesTheServiceTag_ForRequestsWithoutAFormSubmission(string? additionalInfo, bool expected)
    {
        Assert.Equal(expected, RequestedQuote.IsCargoRequest("Bulk Carrier", sourceFormKey: null, additionalInfo));
    }

    [Fact]
    public void IsCargoRequest_TrustsTheFormOverTheTag()
    {
        // A form submission is authoritative: a cargo form request is cargo whatever its notes say.
        Assert.True(RequestedQuote.IsCargoRequest("Dry Bulk", FormDefinition.CargoRequestKey, "[Ship Brokerage] pasted text"));
        Assert.False(RequestedQuote.IsCargoRequest("Bulk Carrier", FormDefinition.ShipRequestKey, "[Cargo Brokerage]"));
    }
}
