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
}
