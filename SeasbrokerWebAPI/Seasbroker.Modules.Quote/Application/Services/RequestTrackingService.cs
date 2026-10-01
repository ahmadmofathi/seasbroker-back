using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Quote.Application.DTOs;
using Seasbroker.Modules.Quote.Application.Exceptions;

namespace Seasbroker.Modules.Quote.Application.Services;

public interface IRequestTrackingService
{
    Task<RequestTrackingDto> TrackAsync(string? number, string? email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Public "track your request" lookup. A request is found by its tracking number (SB-...) or, for
/// cargo, by its listing reference (CRG-...), and only shown when the email matches the customer's -
/// a wrong number and a wrong email give the same answer, so neither can be probed.
/// </summary>
public class RequestTrackingService : IRequestTrackingService
{
    public const string CargoService = "Cargo Brokerage";
    public const string ShipService = "Ship Brokerage";
    public const string ClearanceService = "Customs Clearance";
    public const string OtherService = "Request";

    private readonly SeasbrokerDbContext _dbContext;

    public RequestTrackingService(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RequestTrackingDto> TrackAsync(string? number, string? email, CancellationToken cancellationToken = default)
    {
        var normalizedNumber = number?.Trim().ToUpperInvariant() ?? string.Empty;
        var normalizedEmail = email?.Trim() ?? string.Empty;
        if (normalizedNumber.Length == 0 || normalizedEmail.Length == 0)
        {
            throw new QuoteException("Enter your tracking number and email address.", StatusCodes.Status400BadRequest);
        }

        var quote = await _dbContext.RequestedQuotes
            .AsNoTracking()
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.TrackingNumber == normalizedNumber, cancellationToken);

        CargoListing? listing;
        if (quote is null)
        {
            // Not a request number - maybe the cargo listing reference the team gave the customer.
            listing = await _dbContext.CargoListings
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.RequestedQuote)
                .FirstOrDefaultAsync(c => c.ReferenceNumber == normalizedNumber, cancellationToken);
            quote = listing?.RequestedQuote;
        }
        else
        {
            listing = await _dbContext.CargoListings
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.RequestedQuoteId == quote.Id, cancellationToken);
        }

        var customerEmail = quote?.Customer?.Email ?? listing?.Customer?.Email;
        if ((quote is null && listing is null) ||
            !string.Equals(customerEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new QuoteException(
                "We couldn't find a request with that tracking number and email address.",
                StatusCodes.Status404NotFound);
        }

        Vessel? vessel = null;
        string? sourceFormKey = null;
        if (quote is not null)
        {
            vessel = await _dbContext.Vessels
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.RequestedQuoteId == quote.Id, cancellationToken);

            sourceFormKey = await _dbContext.FormSubmissions
                .AsNoTracking()
                .Where(s => s.RequestedQuoteId == quote.Id)
                .Select(s => s.FormVersion.FormDefinition.Key)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var hasApprovedMatch =
            (listing is not null && await HasApprovedMatchAsync(m => m.CargoListingId == listing.Id, cancellationToken)) ||
            (vessel is not null && await HasApprovedMatchAsync(m => m.VesselId == vessel.Id, cancellationToken));

        return Build(quote, listing, vessel, sourceFormKey, hasApprovedMatch);
    }

    private Task<bool> HasApprovedMatchAsync(
        System.Linq.Expressions.Expression<Func<Match, bool>> filter,
        CancellationToken cancellationToken) =>
        _dbContext.Matches
            .AsNoTracking()
            .Where(filter)
            .AnyAsync(m => m.Status == MatchStatus.Approved || m.Status == MatchStatus.Completed, cancellationToken);

    /// <summary>Works out which service the request is for and how far it has got.</summary>
    public static RequestTrackingDto Build(
        RequestedQuote? quote,
        CargoListing? listing,
        Vessel? vessel,
        string? sourceFormKey,
        bool hasApprovedMatch)
    {
        var service = ServiceOf(quote, listing, sourceFormKey);
        var dto = new RequestTrackingDto
        {
            TrackingNumber = quote?.TrackingNumber ?? listing!.ReferenceNumber,
            Service = service,
            SubmittedAt = quote?.Created ?? listing!.Created,
            Type = listing?.CargoType ?? quote?.CargoType,
            DeparturePort = NullIfBlank(listing?.DeparturePort ?? quote?.DeparturePort),
            ArrivalPort = NullIfBlank(listing?.ArrivalPort ?? quote?.ArrivalPort),
            CargoReference = listing?.ReferenceNumber,
            VesselName = vessel?.Name,
        };

        (string Key, string Label)[] stages;
        int reached;
        switch (service)
        {
            case CargoService:
                stages = new[]
                {
                    ("received", "Request received"),
                    ("review", "Under review"),
                    ("listed", "Cargo listed"),
                    ("matched", "Matched with a vessel"),
                    ("completed", "Completed"),
                };
                reached = listing?.Status switch
                {
                    null or CargoStatus.Draft => 1,
                    CargoStatus.Closed => 4,
                    CargoStatus.Matched => 3,
                    _ => hasApprovedMatch ? 3 : 2,
                };
                break;

            case ShipService:
                stages = new[]
                {
                    ("received", "Request received"),
                    ("review", "Under review"),
                    ("fleet", "Vessel added to our fleet"),
                    ("matched", "Matched with cargo"),
                };
                reached = vessel is null ? 1 : hasApprovedMatch ? 3 : 2;
                break;

            default:
                stages = new[] { ("received", "Request received"), ("review", "Under review") };
                reached = 1;
                break;
        }

        if (listing?.Status == CargoStatus.Cancelled)
        {
            dto.Status = "cancelled";
            dto.StatusLabel = "Cancelled";
            dto.StatusDetail = "This cargo listing was cancelled. Please contact us if you have any questions.";
            dto.Steps = stages.Select((s, i) => new TrackingStepDto { Key = s.Key, Label = s.Label, Done = i == 0 }).ToList();
            return dto;
        }

        var last = stages.Length - 1;
        dto.Status = stages[reached].Key;
        dto.StatusLabel = stages[reached].Label;
        dto.StatusDetail = DetailFor(stages[reached].Key, service);
        dto.Steps = stages
            .Select((s, i) => new TrackingStepDto
            {
                Key = s.Key,
                Label = s.Label,
                // The final stage counts as done once reached; earlier ones are done when passed.
                Done = i < reached || (i == reached && reached == last && service != OtherService && service != ClearanceService),
                Current = i == reached,
            })
            .ToList();
        return dto;
    }

    private static string ServiceOf(RequestedQuote? quote, CargoListing? listing, string? sourceFormKey)
    {
        if (sourceFormKey == FormDefinition.CargoRequestKey)
        {
            return CargoService;
        }

        if (sourceFormKey == FormDefinition.ShipRequestKey)
        {
            return ShipService;
        }

        if (sourceFormKey is not null)
        {
            return ClearanceService;
        }

        // No form submission: the old public site tagged the service at the start of the notes.
        var info = quote?.AdditionalInfo?.TrimStart() ?? string.Empty;
        if (info.StartsWith("[Ship Brokerage]", StringComparison.OrdinalIgnoreCase))
        {
            return ShipService;
        }

        if (info.StartsWith("[Customs Clearance]", StringComparison.OrdinalIgnoreCase))
        {
            return ClearanceService;
        }

        if (info.StartsWith("[Contact]", StringComparison.OrdinalIgnoreCase))
        {
            return OtherService;
        }

        return listing is not null || quote is not null && RequestedQuote.IsCargoRequest(quote.CargoType, null, quote.AdditionalInfo)
            ? CargoService
            : OtherService;
    }

    private static string DetailFor(string stage, string service) => stage switch
    {
        "review" => "We've received your request and our team is reviewing it. We'll contact you shortly.",
        "listed" => "Your cargo is listed and we're looking for a suitable vessel.",
        "fleet" => "Your vessel is in our fleet and we're looking for suitable cargo.",
        "matched" when service == ShipService => "Your vessel has been matched with cargo. Our team will contact you with the details.",
        "matched" => "Your cargo has been matched with a vessel. Our team will contact you with the details.",
        "completed" => "This request has been completed. Thank you for working with us.",
        _ => "We've received your request.",
    };

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Trim() is "N/A" ? null : value.Trim();
}
