using System.Net.Http.Json;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Matching.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class MatchingIntegrationTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public MatchingIntegrationTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RunMatching_ForCargo_CreatesPendingApprovalMatch()
    {
        var scenario = await BusinessScenarioBuilder.CreateMatchingScenarioAsync(_fixture);

        var runResponse = await scenario.AdminClient.PostAsJsonAsync(
            "/api/matching/run",
            new RunMatchingRequest { CargoListingId = scenario.Cargo.Id },
            IntegrationJson.Options);

        runResponse.EnsureSuccessStatusCode();

        var result = await runResponse.Content.ReadFromJsonAsync<MatchingRunResultDto>(IntegrationJson.Options);
        Assert.NotNull(result);
        Assert.True(result.MatchesCreated >= 1);
        Assert.Contains(result.Items, item =>
            item.CargoListingId == scenario.Cargo.Id &&
            item.VesselId == scenario.Vessel.Id &&
            item.Status == MatchStatus.PendingApproval);
    }

    [Fact]
    public async Task CreateManualMatch_CreatesPendingApprovalMatch()
    {
        var scenario = await BusinessScenarioBuilder.CreateMatchingScenarioAsync(_fixture);

        var manualResponse = await scenario.AdminClient.PostAsJsonAsync(
            "/api/matching/manual",
            new CreateManualMatchRequest
            {
                CargoListingId = scenario.Cargo.Id,
                VesselId = scenario.Vessel.Id,
                Score = 95m,
                MatchReason = "Integration manual match",
            },
            IntegrationJson.Options);

        manualResponse.EnsureSuccessStatusCode();

        var match = await manualResponse.Content.ReadFromJsonAsync<MatchRecordDto>(IntegrationJson.Options);
        Assert.NotNull(match);
        Assert.Equal(MatchStatus.PendingApproval, match.Status);
        Assert.Equal(MatchSource.Manual, match.Source);
    }
}
