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

    [Theory]
    [InlineData("Dry Bulk", "Bulk")]
    [InlineData("General & Breakbulk Cargo", "General Cargo")]
    [InlineData("Project & Heavy-Lift Cargo", "General Cargo")]
    [InlineData("Containerized Cargo", "Container")]
    [InlineData("RoRo", "RoRo")]
    [InlineData("Liquid Bulk", "Tanker")]
    [InlineData("Gas", "LNG")]
    [InlineData("Refrigerated & Perishable Cargo", "Container")]
    public void Calculate_ScoresTypeMatch_ForCargoFormCargoTypes(string cargoType, string vesselType)
    {
        var departure = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var cargo = new CargoListing
        {
            CargoType = cargoType,
            Weight = 5000,
            DeparturePort = "Rotterdam",
            ArrivalPort = "Singapore",
            DepartureTime = departure,
            ArrivalTime = departure.AddDays(10),
            Priority = 3,
        };
        var vessel = new Vessel { VesselType = vesselType, Dwt = 10000, CurrentPort = "Rotterdam" };
        var availability = new VesselAvailability
        {
            OpenPort = "Rotterdam",
            AvailableFrom = departure,
            AvailableTo = departure.AddDays(10),
            IsActive = true,
        };

        var result = MatchingScoreCalculator.Calculate(cargo, vessel, availability, DefaultWeights);

        Assert.Equal(15m, result.Breakdown[MatchingConstants.CriterionType]);
    }

    private static (CargoListing Cargo, Vessel Vessel, VesselAvailability Availability) RouteScenario(
        string loadingPort,
        string dischargePort,
        params (string Port, int Day)[] route)
    {
        var start = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        var cargo = new CargoListing
        {
            CargoType = "Dry Bulk",
            Weight = 5000,
            DeparturePort = loadingPort,
            ArrivalPort = dischargePort,
            DepartureTime = start.AddDays(2), // cargo ready
            ArrivalTime = start.AddDays(12),  // wanted at discharge by
            Priority = 3,
        };
        var vessel = new Vessel { VesselType = "Bulk", Dwt = 10000, CurrentPort = "Somewhere Else" };
        var availability = new VesselAvailability
        {
            OpenPort = route[0].Port,
            DestinationPort = route[^1].Port,
            AvailableFrom = start,
            AvailableTo = start.AddDays(30),
            IsActive = true,
            RouteStops = route.Select(r => new RouteStop { Port = r.Port, Eta = start.AddDays(r.Day) }).ToList(),
        };
        return (cargo, vessel, availability);
    }

    [Fact]
    public void Route_LoadingThenDischargeInOrder_ScoresFullPortAndDate()
    {
        var (cargo, vessel, availability) = RouteScenario("B", "C", ("A", 1), ("B", 3), ("C", 8));

        var result = MatchingScoreCalculator.Calculate(cargo, vessel, availability, DefaultWeights);

        Assert.Equal(30m, result.Breakdown[MatchingConstants.CriterionPort]);
        Assert.Equal(25m, result.Breakdown[MatchingConstants.CriterionDate]);
    }

    [Fact]
    public void Route_DischargeBeforeLoading_CountsLoadingOnly()
    {
        var (cargo, vessel, availability) = RouteScenario("C", "A", ("A", 1), ("B", 3), ("C", 8));

        var result = MatchingScoreCalculator.Calculate(cargo, vessel, availability, DefaultWeights);

        Assert.Equal(21m, result.Breakdown[MatchingConstants.CriterionPort]); // 0.7 x 30
    }

    [Fact]
    public void Route_VesselReachesLoadingPortBeforeCargoIsReady_ScoresZeroOnDate()
    {
        var (cargo, vessel, availability) = RouteScenario("A", "C", ("A", 1), ("B", 3), ("C", 8));

        var result = MatchingScoreCalculator.Calculate(cargo, vessel, availability, DefaultWeights);

        Assert.Equal(0m, result.Breakdown[MatchingConstants.CriterionDate]);
    }

    [Fact]
    public void Route_ReachesDischargeAfterWantedDate_ScoresHalfOnDate()
    {
        var (cargo, vessel, availability) = RouteScenario("B", "C", ("A", 1), ("B", 3), ("C", 20));

        var result = MatchingScoreCalculator.Calculate(cargo, vessel, availability, DefaultWeights);

        Assert.Equal(12.5m, result.Breakdown[MatchingConstants.CriterionDate]);
    }
}
