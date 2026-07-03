namespace Seasbroker.Infrastructure.Persistence.Entities;

public class CargoListing : AuditableEntity
{
    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public Guid? RequestedQuoteId { get; set; }

    public RequestedQuote? RequestedQuote { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public string CargoType { get; set; } = string.Empty;

    public double Weight { get; set; }

    public string Dimensions { get; set; } = string.Empty;

    public string DeparturePort { get; set; } = string.Empty;

    public DateTime DepartureTime { get; set; }

    public string ArrivalPort { get; set; } = string.Empty;

    public DateTime ArrivalTime { get; set; }

    public string? AdditionalInfo { get; set; }

    public string Status { get; set; } = CargoStatus.Open;

    public int Priority { get; set; } = 3;
}
