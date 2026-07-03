using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Chat.Application.Abstractions;
using Seasbroker.Modules.Chat.Application.Commands;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Application.Exceptions;
using Seasbroker.Modules.Chat.Application.Mapping;
using Seasbroker.Modules.Chat.Application.Queries;

namespace Seasbroker.Modules.Chat.Application.Services;

public class MessageService : IMessageService
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IChatNotificationService _chatNotificationService;

    public MessageService(
        SeasbrokerDbContext dbContext,
        IChatNotificationService chatNotificationService)
    {
        _dbContext = dbContext;
        _chatNotificationService = chatNotificationService;
    }

    public async Task<IReadOnlyList<MessageRecordDto>> GetByChatIdAsync(
        GetMessagesByChatIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(query.ChatId, out var chatId))
        {
            return Array.Empty<MessageRecordDto>();
        }

        var messagesQuery = _dbContext.Messages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId);

        messagesQuery = query.Sort switch
        {
            "-created" => messagesQuery.OrderByDescending(m => m.Created),
            "created" or _ => messagesQuery.OrderBy(m => m.Created),
        };

        var messages = await messagesQuery.ToListAsync(cancellationToken);
        return messages.Select(ChatMapper.ToRecordDto).ToList();
    }

    public async Task<MessageRecordDto> CreateAsAdminAsync(
        CreateAdminMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(command.ChatId, out var chatId))
        {
            throw new ChatException("Bad call to create message", StatusCodes.Status400BadRequest);
        }

        var chatExists = await _dbContext.Chats.AnyAsync(c => c.Id == chatId, cancellationToken);
        if (!chatExists)
        {
            throw new ChatException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        var message = new global::Seasbroker.Infrastructure.Persistence.Entities.Message
        {
            ChatId = chatId,
            Content = command.Content,
            IsAdmin = true,
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var record = ChatMapper.ToRecordDto(message);
        await _chatNotificationService.NotifyMessageCreatedAsync(record, cancellationToken);

        return record;
    }

    public async Task<MessageRecordDto> CreateAsAnonymousAsync(
        CreateAnonymousMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Token) ||
            string.IsNullOrWhiteSpace(command.ChatId) ||
            string.IsNullOrWhiteSpace(command.Content))
        {
            throw new ChatException("Bad call to create message", StatusCodes.Status400BadRequest);
        }

        var chatToken = await _dbContext.ChatTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == command.Token, cancellationToken);

        if (!Guid.TryParse(command.ChatId, out var chatId))
        {
            throw new ChatException("Bad call to create message", StatusCodes.Status400BadRequest);
        }

        switch (ChatTokenValidator.Validate(chatToken, chatId, DateTime.UtcNow))
        {
            case ChatTokenValidationResult.Invalid:
                throw new ChatException("Invalid token", StatusCodes.Status400BadRequest);
            case ChatTokenValidationResult.Expired:
                throw new ChatException("Token expired.", StatusCodes.Status401Unauthorized);
        }

        var message = new global::Seasbroker.Infrastructure.Persistence.Entities.Message
        {
            ChatId = chatId,
            Content = command.Content,
            IsAdmin = false,
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var record = ChatMapper.ToRecordDto(message);
        await _chatNotificationService.NotifyMessageCreatedAsync(record, cancellationToken);

        return record;
    }
}
