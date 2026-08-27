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
}
