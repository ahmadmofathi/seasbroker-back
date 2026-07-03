using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Application.Abstractions;
using Seasbroker.Modules.Matching.Application.Constants;
using Seasbroker.Modules.Matching.Application.Engine;
using Seasbroker.Modules.Matching.Application.Services;
using Seasbroker.Modules.Matching.Infrastructure;
using Seasbroker.Modules.Matching.Infrastructure.Options;

namespace Seasbroker.Modules.Matching.Tests;

public class MatchingScoreCalculatorTests
{
    private static readonly IReadOnlyDictionary<string, decimal> DefaultWeights =
        new Dictionary<string, decimal>
        {
            [MatchingConstants.CriterionPort] = 30m,
            [MatchingConstants.CriterionDate] = 25m,
            [MatchingConstants.CriterionCapacity] = 25m,
            [MatchingConstants.CriterionType] = 15m,
            [MatchingConstants.CriterionPriority] = 5m,
        };

    [Fact]
    public void Calculate_ReturnsMaximumScore_WhenAllCriteriaMatch()
    {
        var departure = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var arrival = departure.AddDays(10);

        var cargo = new CargoListing
        {
            CargoType = "Bulk",
            Weight = 5000,
            DeparturePort = "Rotterdam",
            ArrivalPort = "Singapore",
            DepartureTime = departure,
            ArrivalTime = arrival,
            Priority = 5,
        };

        var vessel = new Vessel
        {
            VesselType = "Bulk",
            Dwt = 10000,
            CurrentPort = "Rotterdam",
        };

        var availability = new VesselAvailability
        {
            OpenPort = "Rotterdam",
            DestinationPort = "Singapore",
            AvailableFrom = departure.AddDays(-1),
            AvailableTo = arrival.AddDays(1),
            IsActive = true,
        };

        var result = MatchingScoreCalculator.Calculate(cargo, vessel, availability, DefaultWeights);

        Assert.Equal(100m, result.TotalScore);
    }

    [Fact]
    public void Calculate_ReturnsZeroPortScore_WhenPortsDoNotMatch()
    {
        var departure = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var arrival = departure.AddDays(10);

        var cargo = new CargoListing
        {
            CargoType = "Bulk",
            Weight = 5000,
            DeparturePort = "Hamburg",
            ArrivalPort = "Tokyo",
            DepartureTime = departure,
            ArrivalTime = arrival,
            Priority = 3,
        };

        var vessel = new Vessel
        {
            VesselType = "Bulk",
            Dwt = 10000,
            CurrentPort = "Rotterdam",
        };

        var availability = new VesselAvailability
        {
            OpenPort = "Rotterdam",
            DestinationPort = "Singapore",
            AvailableFrom = departure.AddDays(-1),
            AvailableTo = arrival.AddDays(1),
            IsActive = true,
        };

        var result = MatchingScoreCalculator.Calculate(cargo, vessel, availability, DefaultWeights);

        Assert.True(result.TotalScore < 70m);
        Assert.Equal(0m, result.Breakdown[MatchingConstants.CriterionPort]);
    }

    [Fact]
    public void Calculate_ReturnsZeroTypeScore_WhenTypesIncompatible()
    {
        var departure = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var arrival = departure.AddDays(10);

        var cargo = new CargoListing
        {
            CargoType = "Container",
            Weight = 5000,
            DeparturePort = "Rotterdam",
            ArrivalPort = "Singapore",
            DepartureTime = departure,
            ArrivalTime = arrival,
            Priority = 3,
        };

        var vessel = new Vessel
        {
            VesselType = "Bulk",
            Dwt = 10000,
            CurrentPort = "Rotterdam",
        };

        var availability = new VesselAvailability
        {
            OpenPort = "Rotterdam",
            DestinationPort = "Singapore",
            AvailableFrom = departure.AddDays(-1),
            AvailableTo = arrival.AddDays(1),
            IsActive = true,
        };

        var result = MatchingScoreCalculator.Calculate(cargo, vessel, availability, DefaultWeights);

        Assert.Equal(0m, result.Breakdown[MatchingConstants.CriterionType]);
    }
}
