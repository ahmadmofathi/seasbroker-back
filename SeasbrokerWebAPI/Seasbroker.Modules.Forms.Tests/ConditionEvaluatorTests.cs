using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Services;

namespace Seasbroker.Modules.Forms.Tests;

public class ConditionEvaluatorTests
{
    private static FormFieldDto FieldWithCondition(string sourceKey, string op, string? value, string combinator = "AND") =>
        new()
        {
            Key = "target",
            Visible = true,
            ConditionCombinator = combinator,
            Conditions = { new FormFieldConditionDto { SourceFieldKey = sourceKey, Operator = op, Value = value } },
        };

    [Fact]
    public void Field_With_No_Conditions_Is_Always_Visible()
    {
        var field = new FormFieldDto { Key = "x", Visible = true };
        Assert.True(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?>()));
    }

    [Fact]
    public void Invisible_Field_Never_Shows_Regardless_Of_Conditions()
    {
        var field = FieldWithCondition("cargoType", FormConditionOperator.EqualsOp, "Bulk");
        field.Visible = false;
        var values = new Dictionary<string, string?> { ["cargoType"] = "Bulk" };
        Assert.False(ConditionEvaluator.IsVisible(field, values));
    }

    [Theory]
    [InlineData("Project & Heavy-Lift Cargo", true)]
    [InlineData("Dry Bulk", false)]
    public void Equals_Matches_Cargo_Type_Example_From_Spec(string cargoType, bool expected)
    {
        var field = FieldWithCondition("cargoType", FormConditionOperator.EqualsOp, "Project & Heavy-Lift Cargo");
        var values = new Dictionary<string, string?> { ["cargoType"] = cargoType };
        Assert.Equal(expected, ConditionEvaluator.IsVisible(field, values));
    }

    [Fact]
    public void IsEmpty_Treats_Missing_And_Blank_As_Empty()
    {
        var field = FieldWithCondition("notes", FormConditionOperator.IsEmpty, null);
        Assert.True(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?>()));
        Assert.True(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?> { ["notes"] = "  " }));
        Assert.False(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?> { ["notes"] = "hi" }));
    }

    [Fact]
    public void GreaterThan_Compares_Numerically()
    {
        var field = FieldWithCondition("weight", FormConditionOperator.GreaterThan, "100");
        Assert.True(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?> { ["weight"] = "150" }));
        Assert.False(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?> { ["weight"] = "50" }));
    }

    [Fact]
    public void In_Checks_Membership()
    {
        var field = FieldWithCondition("vesselType", FormConditionOperator.In, "[\"Tanker\",\"Bulk Carrier\"]");
        Assert.True(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?> { ["vesselType"] = "Tanker" }));
        Assert.False(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?> { ["vesselType"] = "Container Ship" }));
    }

    [Fact]
    public void Or_Combinator_Passes_When_Any_Condition_Matches()
    {
        var field = new FormFieldDto
        {
            Key = "target",
            Visible = true,
            ConditionCombinator = FormConditionCombinator.Or,
            Conditions =
            {
                new FormFieldConditionDto { SourceFieldKey = "a", Operator = FormConditionOperator.EqualsOp, Value = "1" },
                new FormFieldConditionDto { SourceFieldKey = "b", Operator = FormConditionOperator.EqualsOp, Value = "2" },
            },
        };

        Assert.True(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?> { ["a"] = "x", ["b"] = "2" }));
        Assert.False(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?> { ["a"] = "x", ["b"] = "y" }));
    }

    [Fact]
    public void And_Combinator_Requires_All_Conditions()
    {
        var field = new FormFieldDto
        {
            Key = "target",
            Visible = true,
            ConditionCombinator = FormConditionCombinator.And,
            Conditions =
            {
                new FormFieldConditionDto { SourceFieldKey = "a", Operator = FormConditionOperator.EqualsOp, Value = "1" },
                new FormFieldConditionDto { SourceFieldKey = "b", Operator = FormConditionOperator.EqualsOp, Value = "2" },
            },
        };

        Assert.True(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?> { ["a"] = "1", ["b"] = "2" }));
        Assert.False(ConditionEvaluator.IsVisible(field, new Dictionary<string, string?> { ["a"] = "1", ["b"] = "x" }));
    }
}
