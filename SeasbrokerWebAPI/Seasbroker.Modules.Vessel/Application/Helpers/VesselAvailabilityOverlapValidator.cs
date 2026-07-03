namespace Seasbroker.Modules.Vessel.Application.Helpers;

public static class VesselAvailabilityOverlapValidator
{
    public static bool HasOverlap(
        DateTime from,
        DateTime to,
        IEnumerable<(DateTime From, DateTime To)> existingActiveWindows)
    {
        foreach (var window in existingActiveWindows)
        {
            if (from < window.To && to > window.From)
            {
                return true;
            }
        }

        return false;
    }

    public static void EnsureNoOverlap(
        DateTime from,
        DateTime to,
        IEnumerable<(Guid Id, DateTime From, DateTime To)> existingActiveWindows,
        Guid? excludeAvailabilityId = null)
    {
        foreach (var window in existingActiveWindows)
        {
            if (excludeAvailabilityId.HasValue && window.Id == excludeAvailabilityId.Value)
            {
                continue;
            }

            if (from < window.To && to > window.From)
            {
                throw new InvalidOperationException("Availability window overlaps with an existing active window.");
            }
        }
    }
}
