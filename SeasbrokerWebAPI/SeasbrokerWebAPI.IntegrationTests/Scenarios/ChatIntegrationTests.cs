using System.Net.Http.Json;
using Seasbroker.Modules.Chat.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;
using ChatListResponse = Seasbroker.Modules.Chat.Application.DTOs.PocketBaseListResponse<Seasbroker.Modules.Chat.Application.DTOs.MessageRecordDto>;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class ChatIntegrationTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public ChatIntegrationTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ChatToken_AnonymousMessage_AndAdminReply_WorkEndToEnd()
    {
        var tokenResponse = await _fixture.Client.PostAsync("/api/get-chat-token", content: null);
        tokenResponse.EnsureSuccessStatusCode();

        var chatToken = await tokenResponse.Content.ReadFromJsonAsync<GetChatTokenResponse>(IntegrationJson.Options);
        Assert.NotNull(chatToken);
        Assert.False(string.IsNullOrWhiteSpace(chatToken.Token));
        Assert.False(string.IsNullOrWhiteSpace(chatToken.ChatId));

        var anonymousMessageResponse = await _fixture.Client.PostAsJsonAsync(
            "/api/collections/messages/records",
            new CreateAnonymousMessageRequest
            {
                Token = chatToken.Token,
                ChatId = chatToken.ChatId,
                Content = "Hello from integration test visitor",
            },
            IntegrationJson.Options);

        anonymousMessageResponse.EnsureSuccessStatusCode();

        var anonymousMessage = await anonymousMessageResponse.Content.ReadFromJsonAsync<MessageRecordDto>(IntegrationJson.Options);
        Assert.NotNull(anonymousMessage);
        Assert.Equal(chatToken.ChatId, anonymousMessage.ChatId);
        Assert.False(anonymousMessage.IsAdmin);

        var superuserToken = await _fixture.Client.LoginSuperuserAsync();
        var adminClient = _fixture.CreateAuthenticatedClient(superuserToken);

        var adminMessageResponse = await adminClient.PostAsJsonAsync(
            "/api/collections/messages/records",
            new CreateAdminMessageRequest
            {
                ChatId = chatToken.ChatId,
                Content = "Hello from integration test admin",
            },
            IntegrationJson.Options);

        adminMessageResponse.EnsureSuccessStatusCode();

        var adminMessage = await adminMessageResponse.Content.ReadFromJsonAsync<MessageRecordDto>(IntegrationJson.Options);
        Assert.NotNull(adminMessage);
        Assert.True(adminMessage.IsAdmin);

        var listResponse = await adminClient.GetAsync(
            $"/api/collections/messages/records?filter=chatId = \"{chatToken.ChatId}\"");

        listResponse.EnsureSuccessStatusCode();

        var messages = await listResponse.Content.ReadFromJsonAsync<ChatListResponse>(IntegrationJson.Options);
        Assert.NotNull(messages);
        Assert.Equal(2, messages.Items.Count);
    }
}
