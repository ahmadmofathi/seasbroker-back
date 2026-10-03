using System.Net;
using System.Net.Http.Json;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Quote.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class TrackingIntegrationTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public TrackingIntegrationTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    private Task<HttpResponseMessage> TrackAsync(string? number, string? email) =>
        _fixture.Client.PostAsJsonAsync("/api/track", new TrackRequestDto { Number = number, Email = email }, IntegrationJson.Options);

    private async Task<RequestTrackingDto> TrackOkAsync(string number, string email)
    {
        var response = await TrackAsync(number, email);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RequestTrackingDto>(IntegrationJson.Options))!;
    }

    [Fact]
    public async Task Customer_Can_Track_A_Request_From_Registration_To_Listing()
    {
        var email = UniqueTestData.Email("track");
        var createResponse = await _fixture.Client.PostAsJsonAsync(
            "/api/quote",
            new CreateQuoteRequest
            {
                CargoType = IntegrationTestDefaults.CargoType,
                Weight = 5000,
                DeparturePort = IntegrationTestDefaults.DeparturePort,
                DepartureTime = IntegrationTestDefaults.DepartureTimeIso,
                ArrivalPort = IntegrationTestDefaults.ArrivalPort,
                ArrivalTime = IntegrationTestDefaults.ArrivalTimeIso,
                Dimensions = "10x10x10",
                Fname = "Track",
                Lname = "Me",
                Email = email,
                PhoneNumber = "+31000000009",
            },
            IntegrationJson.Options);
        createResponse.EnsureSuccessStatusCode();

        // Registering hands the customer a tracking number.
        var created = await createResponse.Content.ReadFromJsonAsync<CreateQuoteResponse>(IntegrationJson.Options);
        Assert.NotNull(created);
        Assert.StartsWith(RequestedQuote.TrackingNumberPrefix, created.TrackingNumber);

        // It works without signing in, and the number isn't case-sensitive.
        var tracking = await TrackOkAsync(created.TrackingNumber.ToLowerInvariant(), email.ToUpperInvariant());
        Assert.Equal(created.TrackingNumber, tracking.TrackingNumber);
        Assert.Equal("Cargo Brokerage", tracking.Service);
        Assert.Equal("review", tracking.Status);
        Assert.False(tracking.CanEdit); // sent through the old quote API: no stored answers to edit
        Assert.Null(tracking.CargoReference);

        // A wrong email and an unknown number look the same: not found.
        Assert.Equal(HttpStatusCode.NotFound, (await TrackAsync(created.TrackingNumber, "someone@else.com")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await TrackAsync("SB-00000000", email)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await TrackAsync("", "")).StatusCode);

        // Once the team promotes it, the customer sees it listed - by either number.
        var adminClient = _fixture.CreateAuthenticatedClient(await _fixture.Client.LoginSuperuserAsync());
        var promoteResponse = await adminClient.PostAsJsonAsync(
            "/api/cargo/promote-from-quote",
            new PromoteQuoteToCargoRequest { RequestedQuoteId = created.RequestedQuoteId, Status = CargoStatus.Open, Priority = 3 },
            IntegrationJson.Options);
        promoteResponse.EnsureSuccessStatusCode();
        var listing = await promoteResponse.Content.ReadFromJsonAsync<CargoListingRecordDto>(IntegrationJson.Options);
        Assert.NotNull(listing);

        var listed = await TrackOkAsync(created.TrackingNumber, email);
        Assert.Equal("listed", listed.Status);
        Assert.Equal(listing.ReferenceNumber, listed.CargoReference);
        Assert.Equal(new[] { true, true, false, false, false }, listed.Steps.Select(s => s.Done));

        var byReference = await TrackOkAsync(listing.ReferenceNumber!, email);
        Assert.Equal(created.TrackingNumber, byReference.TrackingNumber);
        Assert.Equal("listed", byReference.Status);
    }
}
