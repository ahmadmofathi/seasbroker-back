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
}
