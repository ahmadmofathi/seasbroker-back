using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Handlers.Commands;
using Seasbroker.Modules.Vessel.Application.Validators;

namespace Seasbroker.Modules.Vessel.Tests;

public class DeactivateVesselCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_SetsVesselInactive_AndDeactivatesAvailabilities()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new SeasbrokerDbContext(options);

        var vessel = new global::Seasbroker.Infrastructure.Persistence.Entities.Vessel
        {
            Name = "Test Vessel",
            VesselType = "Bulk",
            Dwt = 10000,
            CurrentPort = "Hamburg",
            Status = VesselStatus.Active,
        };

        dbContext.Vessels.Add(vessel);
        await dbContext.SaveChangesAsync();

        dbContext.VesselAvailabilities.Add(new global::Seasbroker.Infrastructure.Persistence.Entities.VesselAvailability
        {
            VesselId = vessel.Id,
            AvailableFrom = DateTime.UtcNow,
            AvailableTo = DateTime.UtcNow.AddDays(7),
            OpenPort = "Hamburg",
            IsActive = true,
        });

        await dbContext.SaveChangesAsync();

        var handler = new DeactivateVesselCommandHandler(dbContext);
        await handler.HandleAsync(new DeactivateVesselCommand(vessel.Id.ToString()));

        var updatedVessel = await dbContext.Vessels.SingleAsync();
        var availability = await dbContext.VesselAvailabilities.SingleAsync();

        Assert.Equal(VesselStatus.Inactive, updatedVessel.Status);
        Assert.False(availability.IsActive);
    }
}
