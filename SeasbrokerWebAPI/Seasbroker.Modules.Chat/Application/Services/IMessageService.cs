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

    /// <summary>A visitor's own conversation, proven by the token they were given for that chat.</summary>
    Task<IReadOnlyList<MessageRecordDto>> GetForVisitorAsync(
        string? chatId,
        string? token,
        CancellationToken cancellationToken = default);

    Task<MessageRecordDto> CreateAsAnonymousAsync(
        CreateAnonymousMessageCommand command,
        CancellationToken cancellationToken = default);
}
