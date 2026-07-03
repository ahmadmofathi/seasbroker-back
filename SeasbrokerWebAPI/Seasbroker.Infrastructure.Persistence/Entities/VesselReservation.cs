namespace Seasbroker.Infrastructure.Persistence.Entities;

public class VesselReservation : AuditableEntity
{
    public Guid MatchId { get; set; }

    public Match Match { get; set; } = null!;

    public Guid VesselId { get; set; }

    public Vessel Vessel { get; set; } = null!;

    public Guid VesselAvailabilityId { get; set; }

    public VesselAvailability VesselAvailability { get; set; } = null!;

    public Guid CargoListingId { get; set; }

    public CargoListing CargoListing { get; set; } = null!;

    public double ReservedWeight { get; set; }

    public bool IsReleased { get; set; }

    public DateTime? ReleasedAt { get; set; }
}
