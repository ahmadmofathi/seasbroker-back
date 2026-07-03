using System.Net;
using System.Net.Http.Json;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;
using VesselListResponse = Seasbroker.Modules.Vessel.Application.DTOs.PocketBaseListResponse<Seasbroker.Modules.Vessel.Application.DTOs.VesselRecordDto>;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class VesselIntegrationTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public VesselIntegrationTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VesselCrud_AndAvailabilityLifecycle_Work()
    {
        var adminClient = _fixture.CreateAuthenticatedClient(await _fixture.Client.LoginSuperuserAsync());
        var ownerEmail = UniqueTestData.Email("vessel-owner");
        await CreateQuoteCustomerAsync(ownerEmail);
        var customerId = await TestDataQueries.GetCustomerIdForEmailAsync(_fixture.Factory.Services, ownerEmail);

        var createResponse = await adminClient.PostAsJsonAsync(
            "/api/collections/vessels/records",
            new CreateVesselRequest
            {
                Name = "Integration Bulk Carrier",
                ImoNumber = IntegrationTestDefaults.ImoNumber(),
                VesselType = IntegrationTestDefaults.CargoType,
                Dwt = 15000,
                CurrentPort = IntegrationTestDefaults.DeparturePort,
                Status = VesselStatus.Active,
                Customer = customerId.ToString(),
            },
            IntegrationJson.Options);

        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<VesselRecordDto>(IntegrationJson.Options);
        Assert.NotNull(created);
        Assert.Equal("Integration Bulk Carrier", created.Name);

        var getResponse = await adminClient.GetAsync($"/api/collections/vessels/records/{created.Id}");
        getResponse.EnsureSuccessStatusCode();

        var patchResponse = await adminClient.PatchAsJsonAsync(
            $"/api/collections/vessels/records/{created.Id}",
            new UpdateVesselRequest
            {
                Name = "Integration Bulk Carrier Updated",
                CurrentPort = IntegrationTestDefaults.DeparturePort,
            },
            IntegrationJson.Options);

        patchResponse.EnsureSuccessStatusCode();

        var updated = await patchResponse.Content.ReadFromJsonAsync<VesselRecordDto>(IntegrationJson.Options);
        Assert.NotNull(updated);
        Assert.Equal("Integration Bulk Carrier Updated", updated.Name);

        var availabilityResponse = await adminClient.PostAsJsonAsync(
            "/api/collections/vesselAvailabilities/records",
            new CreateVesselAvailabilityRequest
            {
                VesselId = created.Id,
                AvailableFrom = IntegrationTestDefaults.DepartureTimeUtc.AddDays(-1),
                AvailableTo = IntegrationTestDefaults.ArrivalTimeUtc.AddDays(1),
                OpenPort = IntegrationTestDefaults.DeparturePort,
                DestinationPort = IntegrationTestDefaults.ArrivalPort,
            },
            IntegrationJson.Options);

        availabilityResponse.EnsureSuccessStatusCode();

        var availability = await availabilityResponse.Content.ReadFromJsonAsync<VesselAvailabilityRecordDto>(IntegrationJson.Options);
        Assert.NotNull(availability);
        Assert.True(availability.IsActive);

        var listResponse = await adminClient.GetAsync("/api/collections/vessels/records?page=1&perPage=50");
        listResponse.EnsureSuccessStatusCode();

        var list = await listResponse.Content.ReadFromJsonAsync<VesselListResponse>(IntegrationJson.Options);
        Assert.NotNull(list);
        Assert.Contains(list.Items, item => item.Id == created.Id);

        var deleteResponse = await adminClient.DeleteAsync($"/api/collections/vessels/records/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await adminClient.GetAsync($"/api/collections/vessels/records/{created.Id}");
        afterDelete.EnsureSuccessStatusCode();

        var deletedVessel = await afterDelete.Content.ReadFromJsonAsync<VesselRecordDto>(IntegrationJson.Options);
        Assert.NotNull(deletedVessel);
        Assert.Equal(VesselStatus.Inactive, deletedVessel.Status);
    }

    private async Task CreateQuoteCustomerAsync(string email)
    {
        var response = await _fixture.Client.PostAsJsonAsync(
            "/api/quote",
            new Seasbroker.Modules.Quote.Application.DTOs.CreateQuoteRequest
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
                Email = email,
                PhoneNumber = "+31000000001",
            },
            IntegrationJson.Options);

        response.EnsureSuccessStatusCode();
    }
}
