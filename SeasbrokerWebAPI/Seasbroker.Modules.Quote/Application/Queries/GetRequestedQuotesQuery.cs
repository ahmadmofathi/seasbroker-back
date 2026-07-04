namespace Seasbroker.Modules.Quote.Application.Queries;

public sealed record GetRequestedQuotesQuery(int Page = 1, int PerPage = 50);
