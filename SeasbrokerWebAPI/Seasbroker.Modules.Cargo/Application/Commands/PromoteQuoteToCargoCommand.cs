namespace Seasbroker.Modules.Cargo.Application.Commands;

public sealed record PromoteQuoteToCargoCommand(
    string RequestedQuoteId,
    string? ReferenceNumber,
    string? Status,
    int? Priority,
    PromoteQuoteOverrides? Overrides = null);

/// <summary>
/// Values the admin corrected before promoting. Each one set here replaces the request's value on
/// the new cargo listing; the original request itself is left exactly as the customer sent it.
/// </summary>
public sealed record PromoteQuoteOverrides(
    string? CargoType = null,
    double? Weight = null,
    string? Dimensions = null,
    string? DeparturePort = null,
    DateTime? DepartureTime = null,
    string? ArrivalPort = null,
    DateTime? ArrivalTime = null,
    string? AdditionalInfo = null);
