using System.Text.Json;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Application.Constants;

namespace Seasbroker.Modules.Matching.Application.Engine;

public sealed class MatchScoreResult
{
    public decimal TotalScore { get; init; }

    public string MatchReason { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, decimal> Breakdown { get; init; } =
        new Dictionary<string, decimal>();

    public string ToBreakdownJson() => JsonSerializer.Serialize(Breakdown);
}

public static class MatchingScoreCalculator
{
    public static MatchScoreResult Calculate(
        CargoListing cargo,
        Vessel vessel,
        VesselAvailability availability,
        IReadOnlyDictionary<string, decimal> ruleWeights)
    {
        var portWeight = GetWeight(ruleWeights, MatchingConstants.CriterionPort, 30m);
        var dateWeight = GetWeight(ruleWeights, MatchingConstants.CriterionDate, 25m);
        var capacityWeight = GetWeight(ruleWeights, MatchingConstants.CriterionCapacity, 25m);
        var typeWeight = GetWeight(ruleWeights, MatchingConstants.CriterionType, 15m);
        var priorityWeight = GetWeight(ruleWeights, MatchingConstants.CriterionPriority, 5m);

        var portRatio = CalculatePortCompatibility(cargo, vessel, availability);
        var dateRatio = CalculateDateOverlap(cargo, availability);
        var capacityRatio = CalculateCapacityCompatibility(cargo, vessel);
        var typeRatio = CalculateTypeCompatibility(cargo, vessel);
        var priorityRatio = CalculatePriorityBoost(cargo);

        var portScore = portWeight * portRatio;
        var dateScore = dateWeight * dateRatio;
        var capacityScore = capacityWeight * capacityRatio;
        var typeScore = typeWeight * typeRatio;
        var priorityScore = priorityWeight * priorityRatio;

        var total = Math.Min(100m, portScore + dateScore + capacityScore + typeScore + priorityScore);

        var breakdown = new Dictionary<string, decimal>
        {
            [MatchingConstants.CriterionPort] = Math.Round(portScore, 2),
            [MatchingConstants.CriterionDate] = Math.Round(dateScore, 2),
            [MatchingConstants.CriterionCapacity] = Math.Round(capacityScore, 2),
            [MatchingConstants.CriterionType] = Math.Round(typeScore, 2),
            [MatchingConstants.CriterionPriority] = Math.Round(priorityScore, 2),
        };

        var reason =
            $"Port {portRatio:P0}, Date {dateRatio:P0}, Capacity {capacityRatio:P0}, Type {typeRatio:P0}, Priority {priorityRatio:P0}";

        return new MatchScoreResult
        {
            TotalScore = Math.Round(total, 2),
            MatchReason = reason,
            Breakdown = breakdown,
        };
    }

    private static decimal GetWeight(
        IReadOnlyDictionary<string, decimal> ruleWeights,
        string criterion,
        decimal defaultWeight)
    {
        return ruleWeights.TryGetValue(criterion, out var weight) ? weight : defaultWeight;
    }

    private static decimal CalculatePortCompatibility(
        CargoListing cargo,
        Vessel vessel,
        VesselAvailability availability)
    {
        var departureMatch =
            PortsEqual(cargo.DeparturePort, availability.OpenPort) ||
            PortsEqual(cargo.DeparturePort, vessel.CurrentPort);

        var arrivalMatch = !string.IsNullOrWhiteSpace(availability.DestinationPort) &&
                           PortsEqual(cargo.ArrivalPort, availability.DestinationPort);

        if (departureMatch && arrivalMatch)
        {
            return 1m;
        }

        if (departureMatch)
        {
            return 0.7m;
        }

        if (arrivalMatch)
        {
            return 0.4m;
        }

        return 0m;
    }

    private static decimal CalculateDateOverlap(CargoListing cargo, VesselAvailability availability)
    {
        var cargoStart = cargo.DepartureTime;
        var cargoEnd = cargo.ArrivalTime;
        var cargoDuration = cargoEnd - cargoStart;

        if (cargoDuration <= TimeSpan.Zero)
        {
            return 0m;
        }

        var overlapStart = cargoStart > availability.AvailableFrom ? cargoStart : availability.AvailableFrom;
        var overlapEnd = cargoEnd < availability.AvailableTo ? cargoEnd : availability.AvailableTo;

        if (overlapStart >= overlapEnd)
        {
            return 0m;
        }

        var overlap = overlapEnd - overlapStart;
        var ratio = (decimal)(overlap.TotalSeconds / cargoDuration.TotalSeconds);
        return Math.Clamp(ratio, 0m, 1m);
    }

    private static decimal CalculateCapacityCompatibility(CargoListing cargo, Vessel vessel)
    {
        if (vessel.Dwt < cargo.Weight)
        {
            return 0m;
        }

        var utilization = (decimal)(cargo.Weight / vessel.Dwt);

        if (utilization <= 0.85m)
        {
            return 1m;
        }

        return Math.Clamp(1m - ((utilization - 0.85m) / 0.15m), 0m, 1m);
    }

    private static decimal CalculateTypeCompatibility(CargoListing cargo, Vessel vessel)
    {
        if (!MatchingConstants.CargoVesselTypeCompatibility.TryGetValue(cargo.CargoType, out var compatibleTypes))
        {
            return 0m;
        }

        return compatibleTypes.Contains(vessel.VesselType) ? 1m : 0m;
    }

    private static decimal CalculatePriorityBoost(CargoListing cargo)
    {
        return Math.Clamp(cargo.Priority / 5m, 0m, 1m);
    }

    private static bool PortsEqual(string left, string right) =>
        string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
}
