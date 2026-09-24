using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Exceptions;
using Seasbroker.Modules.Vessel.Application.Handlers.Commands;
using Seasbroker.Modules.Vessel.Application.Validators;

namespace Seasbroker.Modules.Vessel.Tests;

public class VesselAvailabilityRouteTests
{
    private static readonly DateTime From = new(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = From.AddDays(30);

    private static async Task<(SeasbrokerDbContext Db, Guid VesselId)> SeedAsync()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new SeasbrokerDbContext(options);
        var vessel = new global::Seasbroker.Infrastructure.Persistence.Entities.Vessel
        {
            Name = "Route Vessel",
            VesselType = "Bulk",
            Dwt = 10000,
            CurrentPort = "Alexandria - Egypt",
            Status = VesselStatus.Active,
        };
        dbContext.Vessels.Add(vessel);
        await dbContext.SaveChangesAsync();
        return (dbContext, vessel.Id);
    }

    private static Task<VesselAvailabilityRecordDto> CreateAsync(SeasbrokerDbContext db, Guid vesselId, params (string Port, int Day)[] stops) =>
        new CreateVesselAvailabilityCommandHandler(db, new CreateVesselAvailabilityCommandValidator()).HandleAsync(
            new CreateVesselAvailabilityCommand(
                vesselId.ToString(), From, To, string.Empty, null,
                stops.Select(s => new RouteStopDto { Port = s.Port, Eta = From.AddDays(s.Day) }).ToList()));

    [Fact]
    public async Task Create_WithRoute_StoresStops_AndSyncsOpenAndDestinationPorts()
    {
        var (db, vesselId) = await SeedAsync();
        await using var _ = db;

        var result = await CreateAsync(db, vesselId, ("Alexandria - Egypt", 1), ("Piraeus - Greece", 4), ("Rotterdam - Netherlands", 10));

        Assert.Equal("Alexandria - Egypt", result.OpenPort);
        Assert.Equal("Rotterdam - Netherlands", result.DestinationPort);
        Assert.Equal(new[] { "Alexandria - Egypt", "Piraeus - Greece", "Rotterdam - Netherlands" }, result.RouteStops.Select(s => s.Port));
    }

    [Fact]
    public async Task Create_RejectsEtasThatGoBackwards()
    {
        var (db, vesselId) = await SeedAsync();
        await using var _ = db;

        await Assert.ThrowsAsync<VesselException>(() =>
            CreateAsync(db, vesselId, ("Alexandria - Egypt", 5), ("Piraeus - Greece", 2)));
    }

    [Fact]
    public async Task Create_RejectsEtaOutsideTheWindow()
    {
        var (db, vesselId) = await SeedAsync();
        await using var _ = db;

        await Assert.ThrowsAsync<VesselException>(() =>
            CreateAsync(db, vesselId, ("Alexandria - Egypt", 1), ("Piraeus - Greece", 45)));
    }

    [Fact]
    public async Task Create_RejectsTheSamePortTwiceInARow()
    {
        var (db, vesselId) = await SeedAsync();
        await using var _ = db;

        await Assert.ThrowsAsync<VesselException>(() =>
            CreateAsync(db, vesselId, ("Alexandria - Egypt", 1), ("Alexandria - Egypt", 3)));
    }
}
