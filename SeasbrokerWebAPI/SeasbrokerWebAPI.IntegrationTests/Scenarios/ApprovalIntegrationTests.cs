using System.Net.Http.Json;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Approval.Application.DTOs;
using Seasbroker.Modules.Matching.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;
using ApprovalListResponse = Seasbroker.Modules.Approval.Application.DTOs.PocketBaseListResponse<Seasbroker.Modules.Approval.Application.DTOs.MatchApprovalRecordDto>;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class ApprovalIntegrationTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public ApprovalIntegrationTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ApproveRejectAndCompleteMatch_Work()
    {
        var scenario = await BusinessScenarioBuilder.CreateMatchingScenarioAsync(_fixture);

        var runResponse = await scenario.AdminClient.PostAsJsonAsync(
            "/api/matching/run",
            new RunMatchingRequest { CargoListingId = scenario.Cargo.Id },
            IntegrationJson.Options);

        runResponse.EnsureSuccessStatusCode();
        var runResult = await runResponse.Content.ReadFromJsonAsync<MatchingRunResultDto>(IntegrationJson.Options);
        Assert.NotNull(runResult);
        Assert.NotEmpty(runResult.Items);

        var matchId = runResult.Items[0].Id;

        var pendingResponse = await scenario.AdminClient.GetAsync("/api/matches/pending-approval?page=1&perPage=50");
        pendingResponse.EnsureSuccessStatusCode();

        var pending = await pendingResponse.Content.ReadFromJsonAsync<ApprovalListResponse>(IntegrationJson.Options);
        Assert.NotNull(pending);
        Assert.Contains(pending.Items, item => item.Id == matchId);

        var approveResponse = await scenario.AdminClient.PostAsJsonAsync(
            $"/api/matches/{matchId}/approve",
            new MatchApprovalActionRequest { Reason = "Approved in integration test" },
            IntegrationJson.Options);

        approveResponse.EnsureSuccessStatusCode();

        var approved = await approveResponse.Content.ReadFromJsonAsync<MatchApprovalRecordDto>(IntegrationJson.Options);
        Assert.NotNull(approved);
        Assert.Equal(MatchStatus.Approved, approved.Status);
        Assert.NotNull(approved.ApprovedAt);

        var approvedListResponse = await scenario.AdminClient.GetAsync("/api/matches/approved?page=1&perPage=50");
        approvedListResponse.EnsureSuccessStatusCode();

        var approvedList = await approvedListResponse.Content.ReadFromJsonAsync<ApprovalListResponse>(IntegrationJson.Options);
        Assert.NotNull(approvedList);
        Assert.Contains(approvedList.Items, item => item.Id == matchId);

        var completeResponse = await scenario.AdminClient.PostAsJsonAsync(
            $"/api/matches/{matchId}/complete",
            new MatchApprovalActionRequest { Reason = "Completed in integration test" },
            IntegrationJson.Options);

        completeResponse.EnsureSuccessStatusCode();

        var completed = await completeResponse.Content.ReadFromJsonAsync<MatchApprovalRecordDto>(IntegrationJson.Options);
        Assert.NotNull(completed);
        Assert.Equal(MatchStatus.Completed, completed.Status);
    }

    [Fact]
    public async Task RejectMatch_KeepsCargoOpen()
    {
        var scenario = await BusinessScenarioBuilder.CreateMatchingScenarioAsync(_fixture);

        var manualResponse = await scenario.AdminClient.PostAsJsonAsync(
            "/api/matching/manual",
            new CreateManualMatchRequest
            {
                CargoListingId = scenario.Cargo.Id,
                VesselId = scenario.Vessel.Id,
                Score = 88m,
                MatchReason = "Reject path integration test",
            },
            IntegrationJson.Options);

        manualResponse.EnsureSuccessStatusCode();
        var match = await manualResponse.Content.ReadFromJsonAsync<MatchRecordDto>(IntegrationJson.Options);
        Assert.NotNull(match);

        var rejectResponse = await scenario.AdminClient.PostAsJsonAsync(
            $"/api/matches/{match.Id}/reject",
            new MatchApprovalActionRequest { Reason = "Rejected in integration test" },
            IntegrationJson.Options);

        rejectResponse.EnsureSuccessStatusCode();

        var rejected = await rejectResponse.Content.ReadFromJsonAsync<MatchApprovalRecordDto>(IntegrationJson.Options);
        Assert.NotNull(rejected);
        Assert.Equal(MatchStatus.Rejected, rejected.Status);

        var cargoResponse = await scenario.AdminClient.GetAsync($"/api/collections/cargoListings/records/{scenario.Cargo.Id}");
        cargoResponse.EnsureSuccessStatusCode();

        var cargo = await cargoResponse.Content.ReadFromJsonAsync<Seasbroker.Modules.Cargo.Application.DTOs.CargoListingRecordDto>(IntegrationJson.Options);
        Assert.NotNull(cargo);
        Assert.Equal(CargoStatus.Open, cargo.Status);
    }
}
