using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.Services;
using Seasbroker.Modules.Forms.Infrastructure;

namespace Seasbroker.Modules.Forms.Tests;

public class FormSeedDataTests
{
    [Fact]
    public void RequestQuote_Seed_Is_Structurally_Valid()
    {
        var (_, _, _, schema) = FormSeedData.RequestQuote();
        FormSchemaValidator.Validate(schema);
    }

    [Fact]
    public void RequestRoute_Seed_Is_Structurally_Valid()
    {
        var (_, _, _, schema) = FormSeedData.RequestRoute();
        FormSchemaValidator.Validate(schema);
    }

    [Fact]
    public void RequestClearance_Seed_Is_Structurally_Valid()
    {
        var (_, _, _, schema) = FormSeedData.RequestClearance();
        FormSchemaValidator.Validate(schema);
    }

    [Fact]
    public void RequestQuote_Seed_Has_Project_HeavyLift_Conditional_Fields()
    {
        var (_, _, _, schema) = FormSeedData.RequestQuote();
        var section = schema.Sections.Single(s => s.Key == "project-heavy-lift");
        Assert.True(section.Fields.Count >= 8);
        Assert.All(section.Fields, f => Assert.Contains(f.Conditions, c => c.SourceFieldKey == "cargoType"));
    }

    [Fact]
    public void RequestClearance_Seed_Splits_Import_And_Export_Fields()
    {
        var (_, _, _, schema) = FormSeedData.RequestClearance();
        var importSection = schema.Sections.Single(s => s.Key == "import-details");
        var exportSection = schema.Sections.Single(s => s.Key == "export-details");

        Assert.All(importSection.Fields, f => Assert.Contains(f.Conditions, c => c.SourceFieldKey == "clearanceType" && c.Value == "Import"));
        Assert.All(exportSection.Fields, f => Assert.Contains(f.Conditions, c => c.SourceFieldKey == "clearanceType" && c.Value == "Export"));
    }

    [Theory]
    [InlineData("Gas", null, false, true)]
    [InlineData("Gas", "No", false, true)]
    [InlineData("Dry Bulk", null, true, false)]
    [InlineData("Dry Bulk", "No", true, false)]
    [InlineData("Dry Bulk", "Yes", true, true)]
    public void RequestQuote_Gas_Cargo_Is_Always_Dangerous(string cargoType, string? dangerousGoods, bool questionVisible, bool detailsVisible)
    {
        var (_, _, _, schema) = FormSeedData.RequestQuote();
        var fields = schema.Sections.SelectMany(s => s.Fields).ToList();
        var values = new Dictionary<string, string?> { ["cargoType"] = cargoType, ["dangerousGoods"] = dangerousGoods };

        Assert.Equal(questionVisible, ConditionEvaluator.IsVisible(fields.Single(f => f.Key == "dangerousGoods"), values));
        Assert.All(
            schema.Sections.Single(s => s.Key == "dangerous-goods").Fields,
            f => Assert.Equal(detailsVisible, ConditionEvaluator.IsVisible(f, values)));
    }

    [Fact]
    public void RequestQuote_Arrival_Must_Come_After_Cargo_Ready_Date()
    {
        var (_, _, _, schema) = FormSeedData.RequestQuote();
        var arrival = schema.Sections.SelectMany(s => s.Fields).Single(f => f.Key == "estimatedArrivalDate");

        Assert.Equal("cargoReadyDate", arrival.Validation?.AfterField);
    }

    [Fact]
    public void Every_Seeded_Date_Field_Rejects_Past_Dates()
    {
        var dateFields = new[] { FormSeedData.RequestQuote(), FormSeedData.RequestRoute(), FormSeedData.RequestClearance() }
            .SelectMany(s => s.Item4.Sections)
            .SelectMany(s => s.Fields)
            .Where(f => f.Type == FormFieldType.Date || f.Type == FormFieldType.DateTime)
            .ToList();

        Assert.NotEmpty(dateFields);
        Assert.All(dateFields, f => Assert.True(f.Validation?.NoPastDates == true, $"'{f.Key}' allows past dates."));
    }

    [Fact]
    public void RequestRoute_Scheduled_Route_Is_A_Route_Field_Of_At_Least_Two_Ports()
    {
        var (_, _, _, schema) = FormSeedData.RequestRoute();
        var route = schema.Sections.SelectMany(s => s.Fields).Single(f => f.Key == "schedRoute");

        Assert.Equal(FormFieldType.Route, route.Type);
        Assert.Equal(2, route.Validation?.MinSelections);
        Assert.True(route.Validation?.NoPastDates);
    }

    [Fact]
    public void Upgrades_Are_Already_Part_Of_The_Seed()
    {
        // A freshly seeded form must need no upgrade - otherwise seed and upgrade have drifted apart.
        foreach (var (key, _, _, schema) in new[] { FormSeedData.RequestQuote(), FormSeedData.RequestRoute(), FormSeedData.RequestClearance() })
        {
            Assert.False(FormSchemaUpgrades.Apply(key, schema), $"'{key}' seed is missing an upgrade.");
        }
    }

    [Fact]
    public void Upgrades_Keep_Admin_Edits_And_Only_Patch_Their_Own_Fields()
    {
        var (key, _, _, schema) = FormSeedData.RequestQuote();
        var documents = schema.Sections.SelectMany(s => s.Fields).Single(f => f.Key == "documentsFile");
        documents.Type = FormFieldType.File;          // the old, pre-upgrade shape
        documents.Label = "Photos (admin renamed)";    // an admin edit that must survive

        Assert.True(FormSchemaUpgrades.Apply(key, schema));
        Assert.Equal(FormFieldType.MultiFile, documents.Type);
        Assert.Equal("Photos (admin renamed)", documents.Label);
        FormSchemaValidator.Validate(schema);
    }
}
