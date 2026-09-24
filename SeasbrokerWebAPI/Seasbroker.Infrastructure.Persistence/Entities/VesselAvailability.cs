namespace Seasbroker.Infrastructure.Persistence.Entities;

public class VesselAvailability : AuditableEntity
{
    public Guid VesselId { get; set; }

    public Vessel Vessel { get; set; } = null!;

    public DateTime AvailableFrom { get; set; }

    public DateTime AvailableTo { get; set; }

    public string OpenPort { get; set; } = string.Empty;

    public string? DestinationPort { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The ports the vessel will call at, in order, each with an ETA. When set, OpenPort is the first
    /// stop and DestinationPort the last, kept in sync so older readers still see a sensible from/to.
    /// Empty for availability windows created before routes existed.
    /// </summary>
    public List<RouteStop> RouteStops { get; set; } = new();
}
