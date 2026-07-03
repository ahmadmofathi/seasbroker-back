using System.Net.Http.Json;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Notifications.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class NotificationsIntegrationTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public NotificationsIntegrationTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MatchingWorkflow_CreatesSuperuserNotifications_AndSupportsInboxActions()
    {
        var scenario = await BusinessScenarioBuilder.CreateMatchingScenarioAsync(_fixture);

        var runResponse = await scenario.AdminClient.PostAsJsonAsync(
            "/api/matching/run",
            new RunMatchingRequest { CargoListingId = scenario.Cargo.Id },
            IntegrationJson.Options);

        runResponse.EnsureSuccessStatusCode();

        var notificationsResponse = await scenario.AdminClient.GetAsync("/api/notifications?page=1&perPage=50");
        notificationsResponse.EnsureSuccessStatusCode();

        var notifications = await notificationsResponse.Content.ReadFromJsonAsync<NotificationListResponse>(IntegrationJson.Options);
        Assert.NotNull(notifications);
        Assert.True(notifications.TotalItems >= 1);

        var unreadResponse = await scenario.AdminClient.GetAsync("/api/notifications/unread?page=1&perPage=50");
        unreadResponse.EnsureSuccessStatusCode();

        var unread = await unreadResponse.Content.ReadFromJsonAsync<NotificationListResponse>(IntegrationJson.Options);
        Assert.NotNull(unread);
        Assert.True(unread.TotalItems >= 1);

        var firstNotification = unread.Items[0];

        var markReadResponse = await scenario.AdminClient.PostAsync(
            $"/api/notifications/{firstNotification.Id}/read",
            content: null);

        markReadResponse.EnsureSuccessStatusCode();

        var readNotification = await markReadResponse.Content.ReadFromJsonAsync<NotificationDto>(IntegrationJson.Options);
        Assert.NotNull(readNotification);
        Assert.NotNull(readNotification.ReadAt);

        var markAllResponse = await scenario.AdminClient.PostAsync("/api/notifications/read-all", content: null);
        markAllResponse.EnsureSuccessStatusCode();

        var deleteResponse = await scenario.AdminClient.DeleteAsync($"/api/notifications/{firstNotification.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
