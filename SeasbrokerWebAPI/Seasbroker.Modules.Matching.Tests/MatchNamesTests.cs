using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Application.Queries;
using Seasbroker.Modules.Matching.Application.Services;

namespace Seasbroker.Modules.Matching.Tests;

public class MatchNamesTests
{
    [Fact]
    public async Task Match_List_Includes_Cargo_And_Vessel_Details()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new SeasbrokerDbContext(options);

        var customer = new Customer { Email = "c@test.com", PhoneNumber = "1", FirstName = "Ahmed", LastName = "Fathi" };
        var cargo = new CargoListing
        {
            CustomerId = customer.Id,
            ReferenceNumber = "CRG-20260927-000010",
            CargoType = "Dry Bulk",
            Weight = 30000,
            DeparturePort = "El Iskandariya (Alexandria) - Egypt",
            ArrivalPort = "Jebel Ali - United Arab Emirates",
            DepartureTime = DateTime.UtcNow,
            ArrivalTime = DateTime.UtcNow.AddDays(10),
        };
        var vessel = new Vessel { Name = "lady darla", VesselType = "Bulk", Dwt = 50000, ImoNumber = "1234567", CurrentPort = "Jebel Ali - United Arab Emirates" };
        db.AddRange(customer, cargo, vessel);
        db.Matches.Add(new Match { CargoListingId = cargo.Id, VesselId = vessel.Id, Score = 89, Status = MatchStatus.PendingApproval });
        await db.SaveChangesAsync();

        var result = await new MatchQueryService(db).GetAllAsync(new GetMatchesQuery());

        var match = Assert.Single(result.Items);
        Assert.Equal("CRG-20260927-000010", match.CargoReference);
        Assert.Equal("Dry Bulk", match.CargoType);
        Assert.Equal("El Iskandariya (Alexandria) - Egypt → Jebel Ali - United Arab Emirates", match.CargoRoute);
        Assert.Equal("Ahmed Fathi", match.CustomerName);
        Assert.Equal("lady darla", match.VesselName);
        Assert.Equal(50000, match.VesselDwt);
        Assert.Equal("1234567", match.VesselImo);
    }
}
