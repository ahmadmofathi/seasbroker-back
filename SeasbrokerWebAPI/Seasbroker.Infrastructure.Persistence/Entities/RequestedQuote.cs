namespace Seasbroker.Infrastructure.Persistence.Entities;

public class RequestedQuote : AuditableEntity
{
    /// <summary>The public Contact form saves its messages as quotes with this cargo type.</summary>
    public const string ContactInquiryCargoType = "Contact Inquiry";

    /// <summary>
    /// Service tags the old public site put at the start of AdditionalInfo (e.g. "[Ship Brokerage] ...")
    /// for requests that carry no cargo. Requests sent before the Forms module have no form
    /// submission, so this tag is the only way to tell what they were.
    /// </summary>
    private static readonly string[] NonCargoServiceTags = { "[Ship Brokerage]", "[Customs Clearance]", "[Contact]" };

    /// <summary>
    /// Ship, Clearance and Contact requests are stored as quotes too, but they carry no real cargo.
    /// A quote can become a cargo listing when it came from the Cargo Brokerage form, or - for
    /// requests with no form submission (<paramref name="sourceFormKey"/> null: the direct quote API
    /// and the old public forms) - when it isn't a Contact message or tagged as a ship/clearance request.
    /// </summary>
    public static bool IsCargoRequest(string cargoType, string? sourceFormKey, string? additionalInfo = null)
    {
        if (sourceFormKey is not null)
        {
            return sourceFormKey == FormDefinition.CargoRequestKey;
        }

        if (string.Equals(cargoType, ContactInquiryCargoType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var info = additionalInfo?.TrimStart() ?? string.Empty;
        return !NonCargoServiceTags.Any(tag => info.StartsWith(tag, StringComparison.OrdinalIgnoreCase));
    }

    public const string TrackingNumberPrefix = "SB-";

    /// <summary>A new customer-facing tracking number, e.g. "SB-7C4A9F2E".</summary>
    public static string NewTrackingNumber() =>
        TrackingNumberPrefix + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

    /// <summary>
    /// What the customer is shown after registering and uses (with their email) to follow the
    /// request on the public tracking page. Every request gets one when it's created.
    /// </summary>
    public string TrackingNumber { get; set; } = NewTrackingNumber();

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
