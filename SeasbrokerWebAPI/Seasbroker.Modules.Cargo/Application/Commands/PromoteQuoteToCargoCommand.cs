namespace Seasbroker.Modules.Cargo.Application.Commands;

public sealed record PromoteQuoteToCargoCommand(
    string RequestedQuoteId,
    string? ReferenceNumber,
    string? Status,
    int? Priority);
