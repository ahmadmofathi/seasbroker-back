namespace Seasbroker.Modules.Vessel.Application.Queries;

public sealed record GetVesselsQuery(
    string? Status = null,
    int Page = 1,
    int PerPage = 50);
