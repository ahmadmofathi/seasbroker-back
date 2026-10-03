using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.Constants;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Exceptions;
using Seasbroker.Modules.Forms.Application.Mapping;
using Seasbroker.Modules.Forms.Application.Services;

namespace Seasbroker.Modules.Forms.Tests;

public class RequestEditTests
{
    private sealed class NoFiles : IFileStorageService
    {
        public Task<string> SaveAsync(IFormFile file, string subFolder, CancellationToken cancellationToken = default) =>
            Task.FromResult("unused");

        public Stream OpenRead(string relativePath) => Stream.Null;
    }

    private static FormFieldDto Field(string key, string type, int order, string? systemKey = null, bool required = true, FormFieldValidationDto? validation = null) => new()
    {
        Key = key,
        Label = key,
        Type = type,
        Required = required,
        Visible = true,
        Order = order,
        Width = "Full",
        IsSystemField = systemKey is not null,
        SystemFieldKey = systemKey,
        Validation = validation,
    };

    private static FormSchemaDto SmallCargoForm() => new()
    {
        Sections =
        {
            new FormSectionDto
            {
                Key = "all",
                Label = "All",
                Visible = true,
                Fields =
                {
                    Field("cargoType", FormFieldType.Text, 0, FormsConstants.SystemFieldKeys.CargoType),
                    Field("weight", FormFieldType.Number, 1, FormsConstants.SystemFieldKeys.Weight, validation: new FormFieldValidationDto { Min = 1 }),
                    Field("from", FormFieldType.Text, 2, FormsConstants.SystemFieldKeys.DeparturePort),
                    Field("to", FormFieldType.Text, 3, FormsConstants.SystemFieldKeys.ArrivalPort),
                    Field("ready", FormFieldType.Date, 4, FormsConstants.SystemFieldKeys.DepartureTime),
                    Field("arrive", FormFieldType.Date, 5, FormsConstants.SystemFieldKeys.ArrivalTime),
                    Field("fragile", FormFieldType.Checkbox, 6, required: false),
                    Field("notes", FormFieldType.Textarea, 7, FormsConstants.SystemFieldKeys.AdditionalInfo, required: false),
                    Field("first", FormFieldType.Text, 8, FormsConstants.SystemFieldKeys.FirstName),
                    Field("phone", FormFieldType.Text, 9, FormsConstants.SystemFieldKeys.PhoneNumber),
                    Field("email", FormFieldType.Email, 10, FormsConstants.SystemFieldKeys.Email),
                    Field("docs", FormFieldType.MultiFile, 11, required: false),
                },
            },
        },
    };

    private static Dictionary<string, JsonElement> Answers(object values) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(values))!;

    private static object Original(string email = "maya@test.com") => new
    {
        cargoType = "Dry Bulk",
        weight = "500",
        from = "Hamburg",
        to = "Dubai",
        ready = "2027-03-01",
        arrive = "2027-03-20",
        fragile = true,
        notes = "first version",
        first = "Maya",
        phone = "+20100",
        email,
    };

    private static async Task<(SeasbrokerDbContext Db, FormSubmissionService Service, string Tracking)> RegisterAsync(
        Action<SeasbrokerDbContext, FormVersion>? beforeSubmit = null)
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new SeasbrokerDbContext(options);

        var definition = new FormDefinition { Key = FormsConstants.FormKeys.RequestQuote, Name = "Cargo" };
        var version = FormMapper.ToNewVersion(definition.Id, 1, FormVersionStatus.Published, SmallCargoForm());
        db.FormDefinitions.Add(definition);
        db.FormVersions.Add(version);
        await db.SaveChangesAsync();
        beforeSubmit?.Invoke(db, version);

        var service = new FormSubmissionService(db, new NoFiles());
        var response = await service.SubmitAsync(FormsConstants.FormKeys.RequestQuote, Answers(Original()), new FormFileCollection());
        return (db, service, response.TrackingNumber!);
    }

    [Fact]
    public async Task Load_Returns_The_Form_And_The_Answers_In_The_Shape_The_Inputs_Use()
    {
        var (db, service, tracking) = await RegisterAsync();
        await using var _ = db;

        var form = await service.LoadForEditAsync(tracking.ToLowerInvariant(), " MAYA@test.com ");

        Assert.Equal(tracking, form.TrackingNumber);
        Assert.Contains(form.Schema.Sections.SelectMany(s => s.Fields), f => f.Key == "weight");
        Assert.Equal("500", form.Values["weight"]);
        Assert.Equal(true, form.Values["fragile"]); // a checkbox comes back as a boolean, not "true"
        Assert.Equal("maya@test.com", form.Values["email"]);
    }

    [Fact]
    public async Task Update_Changes_The_Request_The_Answers_And_The_Contact_But_Never_The_Email()
    {
        var (db, service, tracking) = await RegisterAsync();
        await using var _ = db;

        await service.UpdateAsync(
            tracking,
            "maya@test.com",
            Answers(new
            {
                cargoType = "Dry Bulk",
                weight = "750",
                from = "Rotterdam",
                to = "Dubai",
                ready = "2027-03-05",
                arrive = "2027-03-25",
                fragile = false,
                notes = "second version",
                first = "Maya Ahmed",
                phone = "+20111",
                email = "someone-else@test.com", // must be ignored: the email identifies the customer
            }));

        var quote = await db.RequestedQuotes.Include(q => q.Customer).SingleAsync();
        Assert.Equal(750, quote.Weight);
        Assert.Equal("Rotterdam", quote.DeparturePort);
        Assert.Equal("2027-03-05", quote.DepartureTime);
        Assert.Contains("second version", quote.AdditionalInfo);
        Assert.NotNull(quote.CustomerEditedAt);
        Assert.Equal(tracking, quote.TrackingNumber);

        Assert.Equal("maya@test.com", quote.Customer.Email);
        Assert.Equal("Maya Ahmed", quote.Customer.FirstName);
        Assert.Equal("+20111", quote.Customer.PhoneNumber);

        var stored = await db.FormSubmissionValues.ToDictionaryAsync(v => v.FieldKey, v => v.ValueText);
        Assert.Equal("750", stored["weight"]);
        Assert.Equal("false", stored["fragile"]);
        Assert.Equal("maya@test.com", stored["email"]);
        Assert.Single(await db.FormSubmissions.ToListAsync()); // edited in place, not a second request
    }

    [Fact]
    public async Task Update_Is_Validated_Like_Registering_And_Leaves_The_Request_Alone_When_It_Fails()
    {
        var (db, service, tracking) = await RegisterAsync();
        await using var _ = db;

        var bad = Answers(new
        {
            cargoType = "Dry Bulk",
            weight = "0", // below the minimum
            from = "Hamburg",
            to = "Dubai",
            ready = "2027-03-01",
            arrive = "2027-03-20",
            first = "Maya",
            phone = "+20100",
            email = "maya@test.com",
        });

        await Assert.ThrowsAsync<FormsException>(() => service.UpdateAsync(tracking, "maya@test.com", bad));

        var quote = await db.RequestedQuotes.AsNoTracking().SingleAsync();
        Assert.Equal(500, quote.Weight);
        Assert.Null(quote.CustomerEditedAt);
    }

    [Fact]
    public async Task A_Wrong_Email_Or_Number_Is_Not_Found_And_Both_Look_The_Same()
    {
        var (db, service, tracking) = await RegisterAsync();
        await using var _ = db;

        var wrongEmail = await Assert.ThrowsAsync<FormsException>(() => service.LoadForEditAsync(tracking, "nobody@test.com"));
        var wrongNumber = await Assert.ThrowsAsync<FormsException>(() => service.LoadForEditAsync("SB-00000000", "maya@test.com"));

        Assert.Equal(404, wrongEmail.StatusCode);
        Assert.Equal(404, wrongNumber.StatusCode);
        Assert.Equal(wrongEmail.Message, wrongNumber.Message);
        await Assert.ThrowsAsync<FormsException>(() => service.UpdateAsync(tracking, "nobody@test.com", Answers(Original())));
    }

    [Fact]
    public async Task Once_The_Team_Accepts_The_Request_It_Can_No_Longer_Be_Edited()
    {
        var (db, service, tracking) = await RegisterAsync();
        await using var _ = db;
        var quote = await db.RequestedQuotes.SingleAsync();

        db.CargoListings.Add(new CargoListing
        {
            CustomerId = quote.CustomerId,
            RequestedQuoteId = quote.Id,
            ReferenceNumber = "CRG-LOCK-001",
            CargoType = "Dry Bulk",
            DepartureTime = DateTime.UtcNow,
            ArrivalTime = DateTime.UtcNow.AddDays(1),
        });
        await db.SaveChangesAsync();

        var load = await Assert.ThrowsAsync<FormsException>(() => service.LoadForEditAsync(tracking, "maya@test.com"));
        var save = await Assert.ThrowsAsync<FormsException>(() => service.UpdateAsync(tracking, "maya@test.com", Answers(Original())));

        Assert.Equal(409, load.StatusCode);
        Assert.Equal(409, save.StatusCode);
        Assert.Equal(500, (await db.RequestedQuotes.AsNoTracking().SingleAsync()).Weight);
    }

    [Fact]
    public async Task A_Request_Without_Stored_Answers_Cannot_Be_Edited()
    {
        var (db, service, _) = await RegisterAsync();
        await using var _db = db;

        // Sent before the Forms module: a quote with no form submission behind it.
        var customer = await db.Customers.SingleAsync();
        var old = new RequestedQuote { CustomerId = customer.Id, CargoType = "Bulk", DeparturePort = "A", ArrivalPort = "B", DepartureTime = "x", ArrivalTime = "y", Dimensions = "1" };
        db.RequestedQuotes.Add(old);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<FormsException>(() => service.LoadForEditAsync(old.TrackingNumber, "maya@test.com"));
        Assert.Equal(409, ex.StatusCode);
    }
}
