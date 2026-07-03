namespace Seasbroker.Infrastructure.Persistence.Entities;

public class RequestedQuote : AuditableEntity
{
    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public string CargoType { get; set; } = string.Empty;

    public double Weight { get; set; }

    public string DeparturePort { get; set; } = string.Empty;

    public string DepartureTime { get; set; } = string.Empty;

    public string ArrivalPort { get; set; } = string.Empty;

    public string ArrivalTime { get; set; } = string.Empty;

    public string Dimensions { get; set; } = string.Empty;

    public string? AdditionalInfo { get; set; }
}
