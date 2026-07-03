using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Chat.Application.Commands;
using Seasbroker.Modules.Chat.Application.Constants;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Application.Abstractions;
using Seasbroker.Modules.Chat.Application.Exceptions;
using Seasbroker.Modules.Chat.Application.Mapping;

namespace Seasbroker.Modules.Chat.Application.Services;

public class ChatTokenService : IChatTokenService
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IGeoLocationService _geoLocationService;
    private readonly IChatNotificationService _chatNotificationService;

    public ChatTokenService(
        SeasbrokerDbContext dbContext,
        IGeoLocationService geoLocationService,
        IChatNotificationService chatNotificationService)
    {
        _dbContext = dbContext;
        _geoLocationService = geoLocationService;
        _chatNotificationService = chatNotificationService;
    }

    public async Task<GetChatTokenResponse> IssueAsync(
        IssueChatTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var location = await _geoLocationService.GetCityCountryAsync(command.RemoteIp, cancellationToken);
        if (string.IsNullOrWhiteSpace(location))
        {
            location = "unknown ip address";
        }

        var chat = new global::Seasbroker.Infrastructure.Persistence.Entities.Chat
        {
            Name = "Anonymous chat with user from " + location,
        };

        var token = Guid.NewGuid().ToString();
        var chatToken = new global::Seasbroker.Infrastructure.Persistence.Entities.ChatToken
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(ChatConstants.ChatTokenExpiryHours),
        };

        _dbContext.Chats.Add(chat);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ChatException("Failed to save chat record", StatusCodes.Status500InternalServerError, ex.Message);
        }

        chatToken.ChatId = chat.Id;
        _dbContext.ChatTokens.Add(chatToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ChatException("Failed to save token record", StatusCodes.Status500InternalServerError, ex.Message);
        }

        await _chatNotificationService.NotifyChatCreatedAsync(
            ChatMapper.ToRecordDto(chat),
            cancellationToken);

        return new GetChatTokenResponse
        {
            Token = token,
            ChatId = chat.Id.ToString(),
        };
    }
}
