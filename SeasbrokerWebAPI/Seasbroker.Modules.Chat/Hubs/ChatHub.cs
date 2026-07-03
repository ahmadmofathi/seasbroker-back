using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Chat.Application.Constants;
using Seasbroker.Modules.Chat.Application.Services;

namespace Seasbroker.Modules.Chat.Hubs;

public class ChatHub : Hub
{
    private readonly SeasbrokerDbContext _dbContext;

    public ChatHub(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize(Policy = ChatConstants.SuperuserPolicy)]
    public async Task JoinAdmin()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ChatNotificationGroups.AdminChats);
        await Groups.AddToGroupAsync(Context.ConnectionId, ChatNotificationGroups.AdminMessages);
    }

    public async Task JoinChat(string chatId, string token)
    {
        if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(token))
        {
            throw new HubException("Invalid chat subscription.");
        }

        if (!Guid.TryParse(chatId, out var parsedChatId))
        {
            throw new HubException("Invalid chat subscription.");
        }

        var chatToken = await _dbContext.ChatTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == token);

        switch (ChatTokenValidator.Validate(chatToken, parsedChatId, DateTime.UtcNow))
        {
            case ChatTokenValidationResult.Invalid:
                throw new HubException("Invalid token.");
            case ChatTokenValidationResult.Expired:
                throw new HubException("Token expired.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, ChatNotificationGroups.ForChat(chatId));
    }
}
