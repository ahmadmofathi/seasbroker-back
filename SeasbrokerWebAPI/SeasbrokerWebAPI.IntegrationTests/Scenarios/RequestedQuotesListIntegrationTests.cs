using System.Net.Http.Json;
using Seasbroker.Modules.Quote.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class RequestedQuotesListIntegrationTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public RequestedQuotesListIntegrationTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ListRequestedQuotes_ReturnsPersistedPublicRequests()
    {
        var email = UniqueTestData.Email("list-quote");
        var token = await _fixture.Client.LoginSuperuserAsync();

        var createResponse = await _fixture.Client.PostAsJsonAsync(
            "/api/quote",
            new CreateQuoteRequest
            {
                CargoType = IntegrationTestDefaults.CargoType,
                Weight = 1200,
                DeparturePort = IntegrationTestDefaults.DeparturePort,
                DepartureTime = IntegrationTestDefaults.DepartureTimeIso,
                ArrivalPort = IntegrationTestDefaults.ArrivalPort,
                ArrivalTime = IntegrationTestDefaults.ArrivalTimeIso,
                Dimensions = "10x10x10",
                AdditionalInfo = "[Contact] Subject: Test. Message: Hello",
                Fname = "List",
                Lname = "Quote",
                Email = email,
                PhoneNumber = "+31000000001",
            },
            IntegrationJson.Options);

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CreateQuoteResponse>(IntegrationJson.Options);
        Assert.NotNull(created);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/collections/requestedQuotes/records?page=1&perPage=50");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var listResponse = await _fixture.Client.SendAsync(request);
        listResponse.EnsureSuccessStatusCode();

        var list = await listResponse.Content.ReadFromJsonAsync<PocketBaseListResponse<RequestedQuoteRecordDto>>(IntegrationJson.Options);
        Assert.NotNull(list);
        Assert.Contains(list!.Items, q => q.Id == created!.Id && q.Email == email && q.Fname == "List");
    }
}
