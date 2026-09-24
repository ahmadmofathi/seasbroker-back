namespace Seasbroker.Infrastructure.Persistence.Entities;

public class RequestedQuote : AuditableEntity
{
    /// <summary>The public Contact form saves its messages as quotes with this cargo type.</summary>
    public const string ContactInquiryCargoType = "Contact Inquiry";

    /// <summary>
    /// Ship, Clearance and Contact requests are stored as quotes too, but they carry no real cargo.
    /// A quote can become a cargo listing when it came from the Cargo Brokerage form, or through the
    /// direct quote API (<paramref name="sourceFormKey"/> null) as long as it isn't a Contact message.
    /// </summary>
    public static bool IsCargoRequest(string cargoType, string? sourceFormKey) =>
        sourceFormKey is null
            ? !string.Equals(cargoType, ContactInquiryCargoType, StringComparison.OrdinalIgnoreCase)
            : sourceFormKey == FormDefinition.CargoRequestKey;

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
