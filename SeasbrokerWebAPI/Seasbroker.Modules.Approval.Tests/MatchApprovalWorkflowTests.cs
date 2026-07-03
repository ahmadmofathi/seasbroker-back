using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Approval.Application.Abstractions;
using Seasbroker.Modules.Approval.Application.Commands;
using Seasbroker.Modules.Approval.Application.Services;
using Seasbroker.Modules.Approval.Application.Validators;

namespace Seasbroker.Modules.Approval.Tests;

public class MatchApprovalWorkflowTests
{
    private static readonly Guid SuperuserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Approve_SetsCargoMatched_ReservesCapacity_AndRejectsCompetingMatches()
    {
        await using var dbContext = CreateDbContext();
        var (cargoId, vesselId, availabilityId, approvedMatchId, competingMatchId) =
            await SeedPendingApprovalScenarioAsync(dbContext);

        var service = CreateApprovalService(dbContext);
        var result = await service.ApproveAsync(
            new ApproveMatchCommand(approvedMatchId.ToString(), SuperuserId, "Approved by broker", null));

        Assert.Equal(MatchStatus.Approved, result.Status);
        Assert.Equal(SuperuserId.ToString(), result.ApprovedBy);

        var cargo = await dbContext.CargoListings.FindAsync(cargoId);
        Assert.Equal(CargoStatus.Matched, cargo!.Status);

        var reservation = await dbContext.VesselReservations.SingleAsync(r => r.MatchId == approvedMatchId);
        Assert.False(reservation.IsReleased);
        Assert.Equal(5000, reservation.ReservedWeight);

        var availability = await dbContext.VesselAvailabilities.FindAsync(availabilityId);
        Assert.False(availability!.IsActive);

        var competing = await dbContext.Matches.FindAsync(competingMatchId);
        Assert.Equal(MatchStatus.Rejected, competing!.Status);
    }

    [Fact]
    public async Task Approve_PreventsSecondApprovalForSameCargo()
    {
        await using var dbContext = CreateDbContext();
        var (_, _, _, firstMatchId, secondMatchId) =
            await SeedPendingApprovalScenarioAsync(dbContext);

        var service = CreateApprovalService(dbContext);
        await service.ApproveAsync(
            new ApproveMatchCommand(firstMatchId.ToString(), SuperuserId, null, null));

        dbContext.ChangeTracker.Clear();

        var secondMatch = await dbContext.Matches.FindAsync(secondMatchId);
        Assert.Equal(MatchStatus.Rejected, secondMatch!.Status);

        await Assert.ThrowsAsync<Application.Exceptions.ApprovalException>(() =>
            service.ApproveAsync(
                new ApproveMatchCommand(secondMatchId.ToString(), SuperuserId, null, null)));
    }

    [Fact]
    public async Task Reject_LeavesCargoOpen()
    {
        await using var dbContext = CreateDbContext();
        var (_, _, _, matchId, _) = await SeedPendingApprovalScenarioAsync(dbContext);

        var service = CreateApprovalService(dbContext);
        var result = await service.RejectAsync(
            new RejectMatchCommand(matchId.ToString(), SuperuserId, "Not suitable", null));

        Assert.Equal(MatchStatus.Rejected, result.Status);

        var cargo = await dbContext.CargoListings.FirstAsync();
        Assert.Equal(CargoStatus.Open, cargo.Status);
    }

    [Fact]
    public async Task CancelApproved_ReleasesCapacity_AndReopensCargo()
    {
        await using var dbContext = CreateDbContext();
        var (cargoId, _, availabilityId, matchId, _) =
            await SeedPendingApprovalScenarioAsync(dbContext);

        var service = CreateApprovalService(dbContext);
        await service.ApproveAsync(
            new ApproveMatchCommand(matchId.ToString(), SuperuserId, null, null));

        dbContext.ChangeTracker.Clear();

        var result = await service.CancelApprovedAsync(
            new CancelMatchCommand(matchId.ToString(), SuperuserId, "Customer withdrew", null));

        Assert.Equal(MatchStatus.Cancelled, result.Status);

        var cargo = await dbContext.CargoListings.FindAsync(cargoId);
        Assert.Equal(CargoStatus.Open, cargo!.Status);

        var reservation = await dbContext.VesselReservations.SingleAsync(r => r.MatchId == matchId);
        Assert.True(reservation.IsReleased);

        var availability = await dbContext.VesselAvailabilities.FindAsync(availabilityId);
        Assert.True(availability!.IsActive);
    }

    [Fact]
    public async Task Complete_LocksMatch_AndClosesCargo()
    {
        await using var dbContext = CreateDbContext();
        var (cargoId, _, _, matchId, _) = await SeedPendingApprovalScenarioAsync(dbContext);

        var service = CreateApprovalService(dbContext);
        await service.ApproveAsync(
            new ApproveMatchCommand(matchId.ToString(), SuperuserId, null, null));

        dbContext.ChangeTracker.Clear();

        var result = await service.CompleteAsync(
            new CompleteMatchCommand(matchId.ToString(), SuperuserId, "Delivered", null));

        Assert.Equal(MatchStatus.Completed, result.Status);

        var cargo = await dbContext.CargoListings.FindAsync(cargoId);
        Assert.Equal(CargoStatus.Closed, cargo!.Status);

        dbContext.ChangeTracker.Clear();

        await Assert.ThrowsAsync<Application.Exceptions.ApprovalException>(() =>
            service.RejectAsync(
                new RejectMatchCommand(matchId.ToString(), SuperuserId, null, null)));
    }

    [Fact]
    public void ApproveMatchCommandValidator_RejectsEmptyMatchId()
    {
        var validator = new ApproveMatchCommandValidator();
        var result = validator.Validate(new ApproveMatchCommand(string.Empty, SuperuserId, null, null));
        Assert.False(result.IsValid);
    }

    private static MatchApprovalService CreateApprovalService(SeasbrokerDbContext dbContext) =>
        new(dbContext, new NoOpDomainEventDispatcher());

    private static SeasbrokerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SeasbrokerDbContext(options);
    }

    private static async Task<(Guid CargoId, Guid VesselId, Guid AvailabilityId, Guid ApprovedMatchId, Guid CompetingMatchId)>
        SeedPendingApprovalScenarioAsync(SeasbrokerDbContext dbContext)
    {
        var departure = DateTime.UtcNow;
        var arrival = departure.AddDays(10);
        var cargoId = Guid.NewGuid();
        var vesselId = Guid.NewGuid();
        var competingVesselId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();
        var competingAvailabilityId = Guid.NewGuid();
        var approvedMatchId = Guid.NewGuid();
        var competingMatchId = Guid.NewGuid();

        dbContext.CargoListings.Add(new CargoListing
        {
            Id = cargoId,
            CustomerId = Guid.NewGuid(),
            ReferenceNumber = "CRG-APPROVAL-001",
            CargoType = "Bulk",
            Weight = 5000,
            Dimensions = "10x10x10",
            DeparturePort = "Rotterdam",
            ArrivalPort = "Singapore",
            DepartureTime = departure,
            ArrivalTime = arrival,
            Status = CargoStatus.Open,
            Priority = 5,
        });

        dbContext.Vessels.AddRange(
            new Vessel
            {
                Id = vesselId,
                Name = "Approval Test Vessel",
                VesselType = "Bulk",
                Dwt = 10000,
                CurrentPort = "Rotterdam",
                Status = VesselStatus.Active,
            },
            new Vessel
            {
                Id = competingVesselId,
                Name = "Competing Vessel",
                VesselType = "Bulk",
                Dwt = 12000,
                CurrentPort = "Rotterdam",
                Status = VesselStatus.Active,
            });

        dbContext.VesselAvailabilities.AddRange(
            new VesselAvailability
            {
                Id = availabilityId,
                VesselId = vesselId,
                OpenPort = "Rotterdam",
                DestinationPort = "Singapore",
                AvailableFrom = departure.AddDays(-1),
                AvailableTo = arrival.AddDays(1),
                IsActive = true,
            },
            new VesselAvailability
            {
                Id = competingAvailabilityId,
                VesselId = competingVesselId,
                OpenPort = "Rotterdam",
                DestinationPort = "Singapore",
                AvailableFrom = departure.AddDays(-1),
                AvailableTo = arrival.AddDays(1),
                IsActive = true,
            });

        dbContext.Matches.AddRange(
            new Match
            {
                Id = approvedMatchId,
                CargoListingId = cargoId,
                VesselId = vesselId,
                Score = 95,
                Status = MatchStatus.PendingApproval,
                Source = MatchSource.Automatic,
                MatchReason = "Top candidate",
            },
            new Match
            {
                Id = competingMatchId,
                CargoListingId = cargoId,
                VesselId = competingVesselId,
                Score = 80,
                Status = MatchStatus.PendingApproval,
                Source = MatchSource.Automatic,
                MatchReason = "Secondary candidate",
            });

        await dbContext.SaveChangesAsync();
        return (cargoId, vesselId, availabilityId, approvedMatchId, competingMatchId);
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
            where TEvent : class => Task.CompletedTask;
    }
}
