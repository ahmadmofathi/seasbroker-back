using Microsoft.EntityFrameworkCore;
using Moq;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Chat.Application.Abstractions;
using Seasbroker.Modules.Chat.Application.Constants;
using Seasbroker.Modules.Chat.Application.Exceptions;
using Seasbroker.Modules.Chat.Application.Services;

namespace Seasbroker.Modules.Chat.Tests;

public class VisitorChatHistoryTests
{
    private static async Task<(SeasbrokerDbContext Db, MessageService Service, Guid ChatId)> SeedAsync(DateTime tokenExpiresAt)
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new SeasbrokerDbContext(options);

        var chatId = Guid.NewGuid();
        var otherChatId = Guid.NewGuid();
        db.Chats.AddRange(
            new global::Seasbroker.Infrastructure.Persistence.Entities.Chat { Id = chatId, Name = "Visitor" },
            new global::Seasbroker.Infrastructure.Persistence.Entities.Chat { Id = otherChatId, Name = "Someone else" });
        db.ChatTokens.Add(new ChatToken { ChatId = chatId, Token = "visitor-token", ExpiresAt = tokenExpiresAt });
        db.Messages.AddRange(
            new Message { ChatId = chatId, Content = "hello", IsAdmin = false, Created = DateTime.UtcNow.AddMinutes(-10) },
            new Message { ChatId = chatId, Content = "we replied while you were away", IsAdmin = true, Created = DateTime.UtcNow.AddMinutes(-5) },
            new Message { ChatId = otherChatId, Content = "someone else's conversation", IsAdmin = false, Created = DateTime.UtcNow.AddMinutes(-3) });
        await db.SaveChangesAsync();

        return (db, new MessageService(db, Mock.Of<IChatNotificationService>()), chatId);
    }

    [Fact]
    public async Task A_Visitor_Reads_Their_Own_Conversation_In_Order_Including_Our_Replies()
    {
        var (db, service, chatId) = await SeedAsync(DateTime.UtcNow.AddHours(1));
        await using var _ = db;

        var messages = await service.GetForVisitorAsync(chatId.ToString(), "visitor-token");

        Assert.Equal(new[] { "hello", "we replied while you were away" }, messages.Select(m => m.Content));
        Assert.Equal(new[] { false, true }, messages.Select(m => m.IsAdmin));
    }

    [Fact]
    public async Task A_Token_Cannot_Read_Another_Chat()
    {
        var (db, service, _) = await SeedAsync(DateTime.UtcNow.AddHours(1));
        await using var _db = db;
        var otherChat = (await db.Chats.SingleAsync(c => c.Name == "Someone else")).Id;

        var ex = await Assert.ThrowsAsync<ChatException>(() => service.GetForVisitorAsync(otherChat.ToString(), "visitor-token"));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task An_Expired_Or_Unknown_Token_Is_Rejected()
    {
        var (db, service, chatId) = await SeedAsync(DateTime.UtcNow.AddMinutes(-1));
        await using var _ = db;

        var expired = await Assert.ThrowsAsync<ChatException>(() => service.GetForVisitorAsync(chatId.ToString(), "visitor-token"));
        var unknown = await Assert.ThrowsAsync<ChatException>(() => service.GetForVisitorAsync(chatId.ToString(), "not-a-token"));
        var missing = await Assert.ThrowsAsync<ChatException>(() => service.GetForVisitorAsync(chatId.ToString(), null));

        Assert.Equal(401, expired.StatusCode);
        Assert.Equal(400, unknown.StatusCode);
        Assert.Equal(400, missing.StatusCode);
    }

    [Fact]
    public void A_Visitors_Chat_Stays_Reachable_For_A_Week()
    {
        Assert.Equal(24 * 7, ChatConstants.ChatTokenExpiryHours);
        Assert.Equal(7 * 24 * 3600, ChatConstants.ChatTokenCookieMaxAgeSeconds);
    }
}
