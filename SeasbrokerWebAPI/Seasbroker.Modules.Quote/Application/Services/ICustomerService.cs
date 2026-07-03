using Seasbroker.Modules.Quote.Application.Commands;
using Seasbroker.Modules.Quote.Application.DTOs;
using Seasbroker.Modules.Quote.Application.Queries;

namespace Seasbroker.Modules.Quote.Application.Services;

public interface ICustomerService
{
    Task<CustomerRecordDto?> GetByEmailAsync(
        GetCustomerByEmailQuery query,
        CancellationToken cancellationToken = default);

    Task<CustomerRecordDto> CreateAsync(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default);
}
