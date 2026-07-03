using Seasbroker.Modules.Chat.Application.Commands;
using Seasbroker.Modules.Chat.Application.DTOs;

namespace Seasbroker.Modules.Chat.Application.Services;

public interface IChatTokenService
{
    Task<GetChatTokenResponse> IssueAsync(IssueChatTokenCommand command, CancellationToken cancellationToken = default);
}
