using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Chat.Application.Abstractions;
using Seasbroker.Modules.Chat.Application.Commands;
using Seasbroker.Modules.Chat.Application.Exceptions;
using Seasbroker.Modules.Chat.Application.Services;

namespace Seasbroker.Modules.Chat.Tests;

public class ChatTokenValidatorTests
{
    [Fact]
    public void Validate_ReturnsExpired_WhenTokenPastExpiry()
    {
        var chatId = Guid.NewGuid();
        var token = new ChatToken
        {
            ChatId = chatId,
            Token = "token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
        };

        var result = ChatTokenValidator.Validate(token, chatId, DateTime.UtcNow);

        Assert.Equal(ChatTokenValidationResult.Expired, result);
    }

    [Fact]
    public void Validate_ReturnsInvalid_WhenChatIdDoesNotMatch()
    {
        var token = new ChatToken
        {
            ChatId = Guid.NewGuid(),
            Token = "token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        var result = ChatTokenValidator.Validate(token, Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(ChatTokenValidationResult.Invalid, result);
    }
}

public class MessageServiceChatTokenTests
{
    [Fact]
    public async Task CreateAsAnonymousAsync_RejectsExpiredToken()
    {
        await using var dbContext = CreateDbContext();
        var chatId = Guid.NewGuid();
        const string tokenValue = "expired-token";

        dbContext.Chats.Add(new global::Seasbroker.Infrastructure.Persistence.Entities.Chat { Id = chatId, Name = "Test chat" });
        dbContext.ChatTokens.Add(new ChatToken
        {
            ChatId = chatId,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
        });
        await dbContext.SaveChangesAsync();

        var notificationService = new Mock<IChatNotificationService>();
        var service = new MessageService(dbContext, notificationService.Object);

        var exception = await Assert.ThrowsAsync<ChatException>(() =>
            service.CreateAsAnonymousAsync(
                new CreateAnonymousMessageCommand(tokenValue, chatId.ToString(), "hello")));

        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
        Assert.Contains("expired", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static SeasbrokerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SeasbrokerDbContext(options);
    }
}
