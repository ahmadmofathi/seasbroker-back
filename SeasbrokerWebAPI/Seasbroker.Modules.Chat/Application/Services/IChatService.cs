using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Application.Queries;

namespace Seasbroker.Modules.Chat.Application.Services;

public interface IChatService
{
    Task<IReadOnlyList<ChatRecordDto>> GetAllAsync(GetChatsQuery query, CancellationToken cancellationToken = default);

    Task<ChatRecordDto> GetByIdAsync(GetChatByIdQuery query, CancellationToken cancellationToken = default);
}
