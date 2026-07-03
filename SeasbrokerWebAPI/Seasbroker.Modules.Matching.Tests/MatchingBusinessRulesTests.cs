using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Application.Abstractions;
using Seasbroker.Modules.Matching.Application.Services;
using Seasbroker.Modules.Matching.Infrastructure;
using Seasbroker.Modules.Matching.Infrastructure.Options;

namespace Seasbroker.Modules.Matching.Tests;

internal sealed class NoOpDomainEventDispatcher : Application.Abstractions.IDomainEventDispatcher
{
    public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : class => Task.CompletedTask;
}

public class MatchingEngineServiceTests
{
    [Fact]
    public async Task RunForCargoAsync_CreatesMatch_WhenScoreMeetsMinimum()
    {
        await using var dbContext = CreateDbContext();
        var (cargoId, _) = await SeedMatchingDataAsync(dbContext);

        var engine = CreateEngine(dbContext, new MatchingOptions { MinScore = 60, MaxProposalsPerCargo = 5 });

        var result = await engine.RunForCargoAsync(cargoId);

        Assert.Equal(1, result.MatchesCreated);
        Assert.Single(result.Items);
        Assert.Equal(MatchStatus.PendingApproval, result.Items[0].Status);
        Assert.Equal(MatchSource.Automatic, result.Items[0].Source);
        Assert.NotNull(result.Items[0].ExpiresAt);
        Assert.True(result.Items[0].ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task RunForCargoAsync_SkipsMatch_WhenDuplicateActiveExists()
    {
        await using var dbContext = CreateDbContext();
        var (cargoId, vesselId) = await SeedMatchingDataAsync(dbContext);

        dbContext.Matches.Add(new Match
        {
            CargoListingId = cargoId,
            VesselId = vesselId,
            Score = 90,
            Status = MatchStatus.PendingApproval,
            Source = MatchSource.Manual,
            MatchReason = "Existing",
        });
        await dbContext.SaveChangesAsync();

        var engine = CreateEngine(dbContext, new MatchingOptions { MinScore = 60, MaxProposalsPerCargo = 5 });

        var result = await engine.RunForCargoAsync(cargoId);

        Assert.Equal(0, result.MatchesCreated);
        Assert.Equal(1, await dbContext.Matches.CountAsync());
    }

    [Fact]
    public async Task RunForCargoAsync_RespectsMaxProposalsPerCargo()
    {
        await using var dbContext = CreateDbContext();
        var cargoId = await SeedCargoWithMultipleVesselsAsync(dbContext, vesselCount: 8);

        var engine = CreateEngine(dbContext, new MatchingOptions { MinScore = 60, MaxProposalsPerCargo = 3 });

        var result = await engine.RunForCargoAsync(cargoId);

        Assert.Equal(3, result.MatchesCreated);
        Assert.Equal(3, await dbContext.Matches.CountAsync());
    }

    [Fact]
    public async Task RunForCargoAsync_ExcludesInactiveVessels()
    {
        await using var dbContext = CreateDbContext();
        var departure = DateTime.UtcNow;
        var arrival = departure.AddDays(10);
        var cargoId = Guid.NewGuid();
        var vesselId = Guid.NewGuid();

        dbContext.CargoListings.Add(new CargoListing
        {
            Id = cargoId,
            CustomerId = Guid.NewGuid(),
            ReferenceNumber = "CRG-TEST-001",
            CargoType = "Bulk",
            Weight = 1000,
            Dimensions = "1x1x1",
            DeparturePort = "Rotterdam",
            ArrivalPort = "Singapore",
            DepartureTime = departure,
            ArrivalTime = arrival,
            Status = CargoStatus.Open,
            Priority = 5,
        });

        dbContext.Vessels.Add(new Vessel
        {
            Id = vesselId,
            Name = "Inactive Vessel",
            VesselType = "Bulk",
            Dwt = 5000,
            CurrentPort = "Rotterdam",
            Status = VesselStatus.Inactive,
        });

        dbContext.VesselAvailabilities.Add(new VesselAvailability
        {
            VesselId = vesselId,
            OpenPort = "Rotterdam",
            DestinationPort = "Singapore",
            AvailableFrom = departure.AddDays(-1),
            AvailableTo = arrival.AddDays(1),
            IsActive = true,
        });

        await dbContext.SaveChangesAsync();

        var engine = CreateEngine(dbContext, new MatchingOptions { MinScore = 60, MaxProposalsPerCargo = 5 });
        var result = await engine.RunForCargoAsync(cargoId);

        Assert.Equal(0, result.MatchesCreated);
    }

    private static MatchingEngineService CreateEngine(SeasbrokerDbContext dbContext, MatchingOptions options)
    {
        return new MatchingEngineService(
            dbContext,
            new NoOpDomainEventDispatcher(),
            Options.Create(options));
    }

    private static SeasbrokerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SeasbrokerDbContext(options);
    }

    private static async Task<(Guid CargoId, Guid VesselId)> SeedMatchingDataAsync(SeasbrokerDbContext dbContext)
    {
        var departure = DateTime.UtcNow;
        var arrival = departure.AddDays(10);
        var cargoId = Guid.NewGuid();
        var vesselId = Guid.NewGuid();

        dbContext.CargoListings.Add(new CargoListing
        {
            Id = cargoId,
            CustomerId = Guid.NewGuid(),
            ReferenceNumber = "CRG-TEST-001",
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

        dbContext.Vessels.Add(new Vessel
        {
            Id = vesselId,
            Name = "Test Bulk Carrier",
            VesselType = "Bulk",
            Dwt = 10000,
            CurrentPort = "Rotterdam",
            Status = VesselStatus.Active,
        });

        dbContext.VesselAvailabilities.Add(new VesselAvailability
        {
            VesselId = vesselId,
            OpenPort = "Rotterdam",
            DestinationPort = "Singapore",
            AvailableFrom = departure.AddDays(-1),
            AvailableTo = arrival.AddDays(1),
            IsActive = true,
        });

        await dbContext.SaveChangesAsync();
        return (cargoId, vesselId);
    }

    private static async Task<Guid> SeedCargoWithMultipleVesselsAsync(SeasbrokerDbContext dbContext, int vesselCount)
    {
        var departure = DateTime.UtcNow;
        var arrival = departure.AddDays(10);
        var cargoId = Guid.NewGuid();

        dbContext.CargoListings.Add(new CargoListing
        {
            Id = cargoId,
            CustomerId = Guid.NewGuid(),
            ReferenceNumber = "CRG-TEST-MULTI",
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

        for (var i = 0; i < vesselCount; i++)
        {
            var vesselId = Guid.NewGuid();
            dbContext.Vessels.Add(new Vessel
            {
                Id = vesselId,
                Name = $"Vessel {i}",
                VesselType = "Bulk",
                Dwt = 10000 + i,
                CurrentPort = "Rotterdam",
                Status = VesselStatus.Active,
            });

            dbContext.VesselAvailabilities.Add(new VesselAvailability
            {
                VesselId = vesselId,
                OpenPort = "Rotterdam",
                DestinationPort = "Singapore",
                AvailableFrom = departure.AddDays(-1),
                AvailableTo = arrival.AddDays(1),
                IsActive = true,
            });
        }

        await dbContext.SaveChangesAsync();
        return cargoId;
    }
}

public class MatchLifecycleTests
{
    [Fact]
    public async Task CreateManualAsync_CreatesPendingApprovalMatch()
    {
        await using var dbContext = CreateDbContext();
        var (cargoId, vesselId) = await SeedDataAsync(dbContext);

        var service = new MatchService(dbContext, new NoOpDomainEventDispatcher());

        var result = await service.CreateManualAsync(
            cargoId.ToString(),
            vesselId.ToString(),
            score: 95m,
            matchReason: "Broker override");

        Assert.Equal(MatchStatus.PendingApproval, result.Status);
        Assert.Equal(MatchSource.Manual, result.Source);
        Assert.Equal(95m, result.Score);
    }

    [Fact]
    public async Task ExpireAsync_TransitionsProposedMatchToExpired()
    {
        await using var dbContext = CreateDbContext();
        var matchId = Guid.NewGuid();
        var cargoId = Guid.NewGuid();
        var vesselId = Guid.NewGuid();

        dbContext.Matches.Add(new Match
        {
            Id = matchId,
            CargoListingId = cargoId,
            VesselId = vesselId,
            Score = 80,
            Status = MatchStatus.Proposed,
            Source = MatchSource.Automatic,
            MatchReason = "Test",
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
        });
        await dbContext.SaveChangesAsync();

        var service = new MatchService(dbContext, new NoOpDomainEventDispatcher());
        var result = await service.ExpireAsync(matchId.ToString());

        Assert.Equal(MatchStatus.Expired, result.Status);
        Assert.Null(result.ExpiresAt);
    }

    [Fact]
    public async Task CancelAsync_TransitionsPendingApprovalToCancelled()
    {
        await using var dbContext = CreateDbContext();
        var matchId = Guid.NewGuid();

        dbContext.Matches.Add(new Match
        {
            Id = matchId,
            CargoListingId = Guid.NewGuid(),
            VesselId = Guid.NewGuid(),
            Score = 80,
            Status = MatchStatus.PendingApproval,
            Source = MatchSource.Automatic,
            MatchReason = "Test",
        });
        await dbContext.SaveChangesAsync();

        var service = new MatchService(dbContext, new NoOpDomainEventDispatcher());
        var result = await service.CancelAsync(matchId.ToString());

        Assert.Equal(MatchStatus.Cancelled, result.Status);
    }

    private static SeasbrokerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SeasbrokerDbContext(options);
    }

    private static async Task<(Guid CargoId, Guid VesselId)> SeedDataAsync(SeasbrokerDbContext dbContext)
    {
        var departure = DateTime.UtcNow;
        var arrival = departure.AddDays(10);
        var cargoId = Guid.NewGuid();
        var vesselId = Guid.NewGuid();

        dbContext.CargoListings.Add(new CargoListing
        {
            Id = cargoId,
            CustomerId = Guid.NewGuid(),
            ReferenceNumber = "CRG-MANUAL-001",
            CargoType = "Bulk",
            Weight = 5000,
            Dimensions = "10x10x10",
            DeparturePort = "Rotterdam",
            ArrivalPort = "Singapore",
            DepartureTime = departure,
            ArrivalTime = arrival,
            Status = CargoStatus.Open,
        });

        dbContext.Vessels.Add(new Vessel
        {
            Id = vesselId,
            Name = "Manual Test Vessel",
            VesselType = "Bulk",
            Dwt = 10000,
            CurrentPort = "Rotterdam",
            Status = VesselStatus.Active,
        });

        dbContext.VesselAvailabilities.Add(new VesselAvailability
        {
            VesselId = vesselId,
            OpenPort = "Rotterdam",
            DestinationPort = "Singapore",
            AvailableFrom = departure.AddDays(-1),
            AvailableTo = arrival.AddDays(1),
            IsActive = true,
        });

        await dbContext.SaveChangesAsync();
        return (cargoId, vesselId);
    }
}

public class MatchExpiryHostedServiceTests
{
    [Fact]
    public async Task ExpireProposedMatchesAsync_ExpiresOnlyProposedPastExpiry()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<SeasbrokerDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.Configure<MatchingOptions>(options => options.ExpiryWorkerIntervalMinutes = 15);

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SeasbrokerDbContext>();

        var expiredMatchId = Guid.NewGuid();
        var activeMatchId = Guid.NewGuid();

        dbContext.Matches.AddRange(
            new Match
            {
                Id = expiredMatchId,
                CargoListingId = Guid.NewGuid(),
                VesselId = Guid.NewGuid(),
                Score = 70,
                Status = MatchStatus.Proposed,
                Source = MatchSource.Automatic,
                MatchReason = "Expired",
                ExpiresAt = DateTime.UtcNow.AddHours(-2),
            },
            new Match
            {
                Id = activeMatchId,
                CargoListingId = Guid.NewGuid(),
                VesselId = Guid.NewGuid(),
                Score = 70,
                Status = MatchStatus.Proposed,
                Source = MatchSource.Automatic,
                MatchReason = "Active",
                ExpiresAt = DateTime.UtcNow.AddHours(24),
            },
            new Match
            {
                Id = Guid.NewGuid(),
                CargoListingId = Guid.NewGuid(),
                VesselId = Guid.NewGuid(),
                Score = 70,
                Status = MatchStatus.PendingApproval,
                Source = MatchSource.Automatic,
                MatchReason = "Not proposed",
            },
            new Match
            {
                Id = Guid.NewGuid(),
                CargoListingId = Guid.NewGuid(),
                VesselId = Guid.NewGuid(),
                Score = 70,
                Status = MatchStatus.PendingApproval,
                Source = MatchSource.Automatic,
                MatchReason = "Expired pending",
                ExpiresAt = DateTime.UtcNow.AddHours(-2),
            });

        await dbContext.SaveChangesAsync();

        var service = new MatchExpiryHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<IOptions<MatchingOptions>>(),
            provider.GetRequiredService<ILogger<MatchExpiryHostedService>>());

        var expiredCount = await service.ExpireProposedMatchesAsync(CancellationToken.None);

        Assert.Equal(2, expiredCount);

        using var verifyScope = provider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<SeasbrokerDbContext>();
        var expired = await verifyContext.Matches.FindAsync(expiredMatchId);
        var active = await verifyContext.Matches.FindAsync(activeMatchId);

        Assert.Equal(MatchStatus.Expired, expired!.Status);
        Assert.Equal(MatchStatus.Proposed, active!.Status);
    }
}

public class MatchingValidatorTests
{
    [Fact]
    public void RunMatchingCommandValidator_RejectsBothScopes()
    {
        var validator = new Application.Validators.RunMatchingCommandValidator();
        var result = validator.Validate(new Application.Commands.RunMatchingCommand("cargo-id", "vessel-id"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateManualMatchCommandValidator_RejectsInvalidScore()
    {
        var validator = new Application.Validators.CreateManualMatchCommandValidator();
        var result = validator.Validate(new Application.Commands.CreateManualMatchCommand(
            "cargo-id",
            "vessel-id",
            Score: 150m,
            MatchReason: null));

        Assert.False(result.IsValid);
    }
}
