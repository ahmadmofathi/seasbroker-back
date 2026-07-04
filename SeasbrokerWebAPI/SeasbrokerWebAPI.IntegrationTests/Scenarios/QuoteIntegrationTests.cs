using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Quote.Application.DTOs;
using SeasbrokerWebAPI.IntegrationTests.Infrastructure;
using SeasbrokerWebAPI.IntegrationTests.Support;

namespace SeasbrokerWebAPI.IntegrationTests.Scenarios;

[Collection(IntegrationTestCollection.Name)]
public sealed class QuoteIntegrationTests
{
    private readonly SqlServerIntegrationFixture _fixture;

    public QuoteIntegrationTests(SqlServerIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateQuote_PersistsCustomerAndQuote()
    {
        var email = UniqueTestData.Email("quote");

        var response = await _fixture.Client.PostAsJsonAsync(
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
                AdditionalInfo = "Integration test quote",
                Fname = "Quote",
                Lname = "Customer",
                Email = email,
                PhoneNumber = "+31000000000",
            },
            IntegrationJson.Options);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CreateQuoteResponse>(IntegrationJson.Options);
        Assert.NotNull(body);
        Assert.Contains("success", body.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(body.Id));
        Assert.Equal(body.Id, body.RequestedQuoteId);

        var quoteId = Guid.Parse(body.Id);

        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SeasbrokerDbContext>();

        var quote = await dbContext.RequestedQuotes.AsNoTracking().SingleAsync(q => q.Id == quoteId);
        Assert.Equal(IntegrationTestDefaults.DeparturePort, quote.DeparturePort);
        Assert.Equal(5000, quote.Weight);
    }
}
