using Seasbroker.Modules.Quote.Application.Commands;
using Seasbroker.Modules.Quote.Application.DTOs;
using Seasbroker.Modules.Quote.Application.Queries;

namespace Seasbroker.Modules.Quote.Application.Services;

public interface IQuoteService
{
    Task<CreateQuoteResponse> CreateAsync(
        CreateQuoteCommand command,
        CancellationToken cancellationToken = default);

    Task<PocketBaseListResponse<RequestedQuoteRecordDto>> GetAllAsync(
        GetRequestedQuotesQuery query,
        CancellationToken cancellationToken = default);

    Task<RequestedQuoteRecordDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);
}
