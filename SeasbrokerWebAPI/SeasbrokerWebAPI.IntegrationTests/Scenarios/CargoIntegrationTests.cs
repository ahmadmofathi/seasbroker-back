using System.Net.Http.Json;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Quote.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;
using CargoListResponse = Seasbroker.Modules.Cargo.Application.DTOs.PocketBaseListResponse<Seasbroker.Modules.Cargo.Application.DTOs.CargoListingRecordDto>;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class CargoIntegrationTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public CargoIntegrationTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PromoteQuote_CreateUpdateCloseCargo_Work()
    {
        var email = UniqueTestData.Email("cargo");
        await _fixture.Client.PostAsJsonAsync(
            "/api/quote",
            BuildQuoteRequest(email),
            IntegrationJson.Options);

        var quoteId = await TestDataQueries.GetLatestQuoteIdForEmailAsync(_fixture.Factory.Services, email);
        var adminClient = _fixture.CreateAuthenticatedClient(await _fixture.Client.LoginSuperuserAsync());

        var promoteResponse = await adminClient.PostAsJsonAsync(
            "/api/cargo/promote-from-quote",
            new PromoteQuoteToCargoRequest
            {
                RequestedQuoteId = quoteId.ToString(),
                ReferenceNumber = UniqueTestData.ReferenceNumber("CRG"),
                Status = CargoStatus.Open,
                Priority = 5,
            },
            IntegrationJson.Options);

        promoteResponse.EnsureSuccessStatusCode();

        var promoted = await promoteResponse.Content.ReadFromJsonAsync<CargoListingRecordDto>(IntegrationJson.Options);
        Assert.NotNull(promoted);
        Assert.Equal(CargoStatus.Open, promoted.Status);

        var patchResponse = await adminClient.PatchAsJsonAsync(
            $"/api/collections/cargoListings/records/{promoted.Id}",
            new UpdateCargoListingRequest
            {
                Priority = 4,
                AdditionalInfo = "Updated by integration test",
            },
            IntegrationJson.Options);

        patchResponse.EnsureSuccessStatusCode();

        var updated = await patchResponse.Content.ReadFromJsonAsync<CargoListingRecordDto>(IntegrationJson.Options);
        Assert.NotNull(updated);
        Assert.Equal(4, updated.Priority);

        var listResponse = await adminClient.GetAsync("/api/collections/cargoListings/records?page=1&perPage=50");
        listResponse.EnsureSuccessStatusCode();

        var list = await listResponse.Content.ReadFromJsonAsync<CargoListResponse>(IntegrationJson.Options);
        Assert.NotNull(list);
        Assert.Contains(list.Items, item => item.Id == promoted.Id);

        var closeResponse = await adminClient.PostAsync($"/api/cargo/{promoted.Id}/close", content: null);
        closeResponse.EnsureSuccessStatusCode();

        var closed = await closeResponse.Content.ReadFromJsonAsync<CargoListingRecordDto>(IntegrationJson.Options);
        Assert.NotNull(closed);
        Assert.Equal(CargoStatus.Closed, closed.Status);
    }

    [Fact]
    public async Task CreateCargoListing_Directly_Works()
    {
        var email = UniqueTestData.Email("cargo-direct");
        await _fixture.Client.PostAsJsonAsync("/api/quote", BuildQuoteRequest(email), IntegrationJson.Options);
        var customerId = await TestDataQueries.GetCustomerIdForEmailAsync(_fixture.Factory.Services, email);

        var adminClient = _fixture.CreateAuthenticatedClient(await _fixture.Client.LoginSuperuserAsync());

        var createResponse = await adminClient.PostAsJsonAsync(
            "/api/collections/cargoListings/records",
            new CreateCargoListingRequest
            {
                Customer = customerId.ToString(),
                ReferenceNumber = UniqueTestData.ReferenceNumber("DIR"),
                CargoType = IntegrationTestDefaults.CargoType,
                Weight = 4200,
                Dimensions = "12x12x12",
                DeparturePort = IntegrationTestDefaults.DeparturePort,
                DepartureTime = IntegrationTestDefaults.DepartureTimeUtc,
                ArrivalPort = IntegrationTestDefaults.ArrivalPort,
                ArrivalTime = IntegrationTestDefaults.ArrivalTimeUtc,
                Status = CargoStatus.Open,
                Priority = 3,
            },
            IntegrationJson.Options);

        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<CargoListingRecordDto>(IntegrationJson.Options);
        Assert.NotNull(created);
        Assert.Equal(CargoStatus.Open, created.Status);
    }

    private static CreateQuoteRequest BuildQuoteRequest(string email) =>
        new()
        {
            CargoType = IntegrationTestDefaults.CargoType,
            Weight = 5000,
            DeparturePort = IntegrationTestDefaults.DeparturePort,
            DepartureTime = IntegrationTestDefaults.DepartureTimeIso,
            ArrivalPort = IntegrationTestDefaults.ArrivalPort,
            ArrivalTime = IntegrationTestDefaults.ArrivalTimeIso,
            Dimensions = "10x10x10",
            Fname = "Cargo",
            Lname = "Owner",
            Email = email,
            PhoneNumber = "+31000000002",
        };
}
