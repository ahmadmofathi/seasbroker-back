using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Application.Exceptions;
using Seasbroker.Modules.Chat.Application.Mapping;
using Seasbroker.Modules.Chat.Application.Queries;

namespace Seasbroker.Modules.Chat.Application.Services;

public class ChatService : IChatService
{
    private readonly SeasbrokerDbContext _dbContext;

    public ChatService(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ChatRecordDto>> GetAllAsync(
        GetChatsQuery query,
        CancellationToken cancellationToken = default)
    {
        var chats = await _dbContext.Chats
            .AsNoTracking()
            .OrderByDescending(c => c.Created)
            .ToListAsync(cancellationToken);

        return chats.Select(ChatMapper.ToRecordDto).ToList();
    }

    public async Task<ChatRecordDto> GetByIdAsync(
        GetChatByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(query.ChatId, out var chatId))
        {
            throw new ChatException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        var chat = await _dbContext.Chats
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);

        if (chat is null)
        {
            throw new ChatException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return ChatMapper.ToRecordDto(chat);
    }
}
