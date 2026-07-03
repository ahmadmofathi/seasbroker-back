using System.Net.Http.Json;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Quote.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;

namespace SeasbrokerWebAPI.IntegrationTests.Support;

internal static class BusinessScenarioBuilder
{
    public static async Task<MatchingScenario> CreateMatchingScenarioAsync(SqlServerIntegrationFixture fixture)
    {
        var cargoEmail = UniqueTestData.Email("cargo-match");
        var vesselEmail = UniqueTestData.Email("vessel-match");

        await fixture.Client.PostAsJsonAsync("/api/quote", BuildQuote(cargoEmail, 5000), IntegrationJson.Options);
        await fixture.Client.PostAsJsonAsync("/api/quote", BuildQuote(vesselEmail, 1000), IntegrationJson.Options);

        var quoteId = await TestDataQueries.GetLatestQuoteIdForEmailAsync(fixture.Factory.Services, cargoEmail);
        var vesselOwnerId = await TestDataQueries.GetCustomerIdForEmailAsync(fixture.Factory.Services, vesselEmail);

        var adminClient = fixture.CreateAuthenticatedClient(await fixture.Client.LoginSuperuserAsync());

        var cargoResponse = await adminClient.PostAsJsonAsync(
            "/api/cargo/promote-from-quote",
            new PromoteQuoteToCargoRequest
            {
                RequestedQuoteId = quoteId.ToString(),
                ReferenceNumber = UniqueTestData.ReferenceNumber("MAT"),
                Status = CargoStatus.Open,
                Priority = 5,
            },
            IntegrationJson.Options);

        cargoResponse.EnsureSuccessStatusCode();
        var cargo = await cargoResponse.Content.ReadFromJsonAsync<CargoListingRecordDto>(IntegrationJson.Options);
        Assert.NotNull(cargo);

        var vesselResponse = await adminClient.PostAsJsonAsync(
            "/api/collections/vessels/records",
            new CreateVesselRequest
            {
                Name = "Matching Integration Vessel",
                ImoNumber = IntegrationTestDefaults.ImoNumber(),
                VesselType = IntegrationTestDefaults.CargoType,
                Dwt = 12000,
                CurrentPort = IntegrationTestDefaults.DeparturePort,
                Status = VesselStatus.Active,
                Customer = vesselOwnerId.ToString(),
            },
            IntegrationJson.Options);

        vesselResponse.EnsureSuccessStatusCode();
        var vessel = await vesselResponse.Content.ReadFromJsonAsync<VesselRecordDto>(IntegrationJson.Options);
        Assert.NotNull(vessel);

        var availabilityResponse = await adminClient.PostAsJsonAsync(
            "/api/collections/vesselAvailabilities/records",
            new CreateVesselAvailabilityRequest
            {
                VesselId = vessel.Id,
                AvailableFrom = IntegrationTestDefaults.DepartureTimeUtc.AddDays(-1),
                AvailableTo = IntegrationTestDefaults.ArrivalTimeUtc.AddDays(1),
                OpenPort = IntegrationTestDefaults.DeparturePort,
                DestinationPort = IntegrationTestDefaults.ArrivalPort,
            },
            IntegrationJson.Options);

        availabilityResponse.EnsureSuccessStatusCode();

        return new MatchingScenario(adminClient, cargo, vessel);
    }

    private static CreateQuoteRequest BuildQuote(string email, double weight) =>
        new()
        {
            CargoType = IntegrationTestDefaults.CargoType,
            Weight = weight,
            DeparturePort = IntegrationTestDefaults.DeparturePort,
            DepartureTime = IntegrationTestDefaults.DepartureTimeIso,
            ArrivalPort = IntegrationTestDefaults.ArrivalPort,
            ArrivalTime = IntegrationTestDefaults.ArrivalTimeIso,
            Dimensions = "10x10x10",
            Fname = "Scenario",
            Lname = "User",
            Email = email,
            PhoneNumber = "+31000000099",
        };
}

internal sealed record MatchingScenario(
    HttpClient AdminClient,
    CargoListingRecordDto Cargo,
    VesselRecordDto Vessel);
