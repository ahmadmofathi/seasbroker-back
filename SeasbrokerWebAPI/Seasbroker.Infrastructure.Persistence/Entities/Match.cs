namespace Seasbroker.Infrastructure.Persistence.Entities;

public class Match : AuditableEntity
{
    public Guid CargoListingId { get; set; }

    public CargoListing CargoListing { get; set; } = null!;

    public Guid VesselId { get; set; }

    public Vessel Vessel { get; set; } = null!;

    public decimal Score { get; set; }

    public string Status { get; set; } = MatchStatus.Proposed;

    public string Source { get; set; } = MatchSource.Automatic;

    public string MatchReason { get; set; } = string.Empty;

    public string? ScoreBreakdown { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public Guid? ChatId { get; set; }

    public Chat? Chat { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? RejectedBy { get; set; }

    public DateTime? RejectedAt { get; set; }

    public Guid? CancelledBy { get; set; }

    public DateTime? CancelledAt { get; set; }

    public Guid? CompletedBy { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Reason { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public VesselReservation? VesselReservation { get; set; }
}
