namespace Seasbroker.Modules.Cargo.Application.Queries;

public sealed record GetCargoListingsQuery(
    string? Status = null,
    string? CustomerId = null,
    int Page = 1,
    int PerPage = 50);
