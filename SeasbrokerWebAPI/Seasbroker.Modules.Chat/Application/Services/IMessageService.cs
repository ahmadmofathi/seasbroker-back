using Seasbroker.Modules.Chat.Application.Commands;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Application.Queries;

namespace Seasbroker.Modules.Chat.Application.Services;

public interface IMessageService
{
    Task<IReadOnlyList<MessageRecordDto>> GetByChatIdAsync(
        GetMessagesByChatIdQuery query,
        CancellationToken cancellationToken = default);

    Task<MessageRecordDto> CreateAsAdminAsync(
        CreateAdminMessageCommand command,
        CancellationToken cancellationToken = default);

    Task<MessageRecordDto> CreateAsAnonymousAsync(
        CreateAnonymousMessageCommand command,
        CancellationToken cancellationToken = default);
}
