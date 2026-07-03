using System.Net.Http.Json;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Approval.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Notifications.Application.DTOs;
using Seasbroker.Modules.Quote.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class EndToEndBusinessWorkflowTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public EndToEndBusinessWorkflowTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CompleteBrokerWorkflow_FromQuoteToCompletedMatch()
    {
        var cargoEmail = UniqueTestData.Email("e2e-cargo");
        var vesselEmail = UniqueTestData.Email("e2e-vessel");

        var quoteResponse = await _fixture.Client.PostAsJsonAsync(
            "/api/quote",
            new CreateQuoteRequest
            {
                CargoType = IntegrationTestDefaults.CargoType,
                Weight = 6000,
                DeparturePort = IntegrationTestDefaults.DeparturePort,
                DepartureTime = IntegrationTestDefaults.DepartureTimeIso,
                ArrivalPort = IntegrationTestDefaults.ArrivalPort,
                ArrivalTime = IntegrationTestDefaults.ArrivalTimeIso,
                Dimensions = "20x20x20",
                AdditionalInfo = "E2E workflow quote",
                Fname = "End",
                Lname = "ToEnd",
                Email = cargoEmail,
                PhoneNumber = "+31000000111",
            },
            IntegrationJson.Options);

        quoteResponse.EnsureSuccessStatusCode();

        await _fixture.Client.PostAsJsonAsync(
            "/api/quote",
            new CreateQuoteRequest
            {
                CargoType = IntegrationTestDefaults.CargoType,
                Weight = 1000,
                DeparturePort = IntegrationTestDefaults.DeparturePort,
                DepartureTime = IntegrationTestDefaults.DepartureTimeIso,
                ArrivalPort = IntegrationTestDefaults.ArrivalPort,
                ArrivalTime = IntegrationTestDefaults.ArrivalTimeIso,
                Dimensions = "1x1x1",
                Fname = "Vessel",
                Lname = "Owner",
                Email = vesselEmail,
                PhoneNumber = "+31000000222",
            },
            IntegrationJson.Options);

        var chatTokenResponse = await _fixture.Client.PostAsync("/api/get-chat-token", content: null);
        chatTokenResponse.EnsureSuccessStatusCode();
        var chatToken = await chatTokenResponse.Content.ReadFromJsonAsync<GetChatTokenResponse>(IntegrationJson.Options);
        Assert.NotNull(chatToken);

        var visitorMessageResponse = await _fixture.Client.PostAsJsonAsync(
            "/api/collections/messages/records",
            new CreateAnonymousMessageRequest
            {
                Token = chatToken.Token,
                ChatId = chatToken.ChatId,
                Content = "Need help with my shipment quote",
            },
            IntegrationJson.Options);

        visitorMessageResponse.EnsureSuccessStatusCode();

        var adminClient = _fixture.CreateAuthenticatedClient(await _fixture.Client.LoginSuperuserAsync());

        var adminMessageResponse = await adminClient.PostAsJsonAsync(
            "/api/collections/messages/records",
            new CreateAdminMessageRequest
            {
                ChatId = chatToken.ChatId,
                Content = "We received your quote request",
            },
            IntegrationJson.Options);

        adminMessageResponse.EnsureSuccessStatusCode();

        var quoteId = await TestDataQueries.GetLatestQuoteIdForEmailAsync(_fixture.Factory.Services, cargoEmail);
        var vesselOwnerId = await TestDataQueries.GetCustomerIdForEmailAsync(_fixture.Factory.Services, vesselEmail);

        var cargoResponse = await adminClient.PostAsJsonAsync(
            "/api/cargo/promote-from-quote",
            new PromoteQuoteToCargoRequest
            {
                RequestedQuoteId = quoteId.ToString(),
                ReferenceNumber = UniqueTestData.ReferenceNumber("E2E"),
                Status = CargoStatus.Open,
                Priority = 5,
            },
            IntegrationJson.Options);

        cargoResponse.EnsureSuccessStatusCode();
        var cargo = await cargoResponse.Content.ReadFromJsonAsync<CargoListingRecordDto>(IntegrationJson.Options);
        Assert.NotNull(cargo);

        var vesselResponse = await adminClient.PostAsJsonAsync(
            "/api/collections/vessels/records",
            new Seasbroker.Modules.Vessel.Application.DTOs.CreateVesselRequest
            {
                Name = "E2E Bulk Carrier",
                ImoNumber = IntegrationTestDefaults.ImoNumber(),
                VesselType = IntegrationTestDefaults.CargoType,
                Dwt = 15000,
                CurrentPort = IntegrationTestDefaults.DeparturePort,
                Status = VesselStatus.Active,
                Customer = vesselOwnerId.ToString(),
            },
            IntegrationJson.Options);

        vesselResponse.EnsureSuccessStatusCode();
        var vessel = await vesselResponse.Content.ReadFromJsonAsync<Seasbroker.Modules.Vessel.Application.DTOs.VesselRecordDto>(IntegrationJson.Options);
        Assert.NotNull(vessel);

        await adminClient.PostAsJsonAsync(
            "/api/collections/vesselAvailabilities/records",
            new Seasbroker.Modules.Vessel.Application.DTOs.CreateVesselAvailabilityRequest
            {
                VesselId = vessel.Id,
                AvailableFrom = IntegrationTestDefaults.DepartureTimeUtc.AddDays(-1),
                AvailableTo = IntegrationTestDefaults.ArrivalTimeUtc.AddDays(1),
                OpenPort = IntegrationTestDefaults.DeparturePort,
                DestinationPort = IntegrationTestDefaults.ArrivalPort,
            },
            IntegrationJson.Options);

        var matchingResponse = await adminClient.PostAsJsonAsync(
            "/api/matching/run",
            new RunMatchingRequest { CargoListingId = cargo.Id },
            IntegrationJson.Options);

        matchingResponse.EnsureSuccessStatusCode();
        var matchingResult = await matchingResponse.Content.ReadFromJsonAsync<MatchingRunResultDto>(IntegrationJson.Options);
        Assert.NotNull(matchingResult);
        Assert.True(matchingResult.MatchesCreated >= 1);

        var matchId = matchingResult.Items[0].Id;

        var notificationsBeforeApproval = await adminClient.GetAsync("/api/notifications/unread?page=1&perPage=50");
        notificationsBeforeApproval.EnsureSuccessStatusCode();
        var notifications = await notificationsBeforeApproval.Content.ReadFromJsonAsync<NotificationListResponse>(IntegrationJson.Options);
        Assert.NotNull(notifications);
        Assert.True(notifications.TotalItems >= 1);

        var approveResponse = await adminClient.PostAsJsonAsync(
            $"/api/matches/{matchId}/approve",
            new MatchApprovalActionRequest { Reason = "E2E approval" },
            IntegrationJson.Options);

        approveResponse.EnsureSuccessStatusCode();
        var approved = await approveResponse.Content.ReadFromJsonAsync<MatchApprovalRecordDto>(IntegrationJson.Options);
        Assert.NotNull(approved);
        Assert.Equal(MatchStatus.Approved, approved.Status);

        var cargoAfterApproval = await adminClient.GetAsync($"/api/collections/cargoListings/records/{cargo.Id}");
        cargoAfterApproval.EnsureSuccessStatusCode();
        var matchedCargo = await cargoAfterApproval.Content.ReadFromJsonAsync<CargoListingRecordDto>(IntegrationJson.Options);
        Assert.NotNull(matchedCargo);
        Assert.Equal(CargoStatus.Matched, matchedCargo.Status);

        var completeResponse = await adminClient.PostAsJsonAsync(
            $"/api/matches/{matchId}/complete",
            new MatchApprovalActionRequest { Reason = "E2E completion" },
            IntegrationJson.Options);

        completeResponse.EnsureSuccessStatusCode();
        var completed = await completeResponse.Content.ReadFromJsonAsync<MatchApprovalRecordDto>(IntegrationJson.Options);
        Assert.NotNull(completed);
        Assert.Equal(MatchStatus.Completed, completed.Status);
    }
}
