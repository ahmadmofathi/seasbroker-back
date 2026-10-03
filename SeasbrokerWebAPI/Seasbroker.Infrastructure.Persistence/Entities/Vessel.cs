namespace Seasbroker.Infrastructure.Persistence.Entities;

public class Vessel : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? ImoNumber { get; set; }

    public string VesselType { get; set; } = string.Empty;

    public double Dwt { get; set; }

    public int? TeuCapacity { get; set; }

    public double? LengthOverall { get; set; }

    public double? Beam { get; set; }

    public double? Draft { get; set; }

    public string CurrentPort { get; set; } = string.Empty;

    public string? FlagCountry { get; set; }

    public string Status { get; set; } = VesselStatus.Active;

    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    /// <summary>The Ship Brokerage request this vessel was added to the fleet from, if any.</summary>
    public Guid? RequestedQuoteId { get; set; }

    public RequestedQuote? RequestedQuote { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// A container ship with reefer plugs can carry refrigerated cargo. False unless someone says
    /// otherwise, so refrigerated cargo is never proposed for a vessel that might not have them.
    /// </summary>
    public bool ReeferPlugs { get; set; }

    public ICollection<VesselAvailability> Availabilities { get; set; } = new List<VesselAvailability>();
}
