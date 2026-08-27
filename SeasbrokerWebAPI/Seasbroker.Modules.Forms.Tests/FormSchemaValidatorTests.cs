using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Exceptions;
using Seasbroker.Modules.Forms.Application.Services;

namespace Seasbroker.Modules.Forms.Tests;

public class FormSchemaValidatorTests
{
    private static FormFieldDto Field(string key, string type = "Text") => new() { Key = key, Label = key, Type = type, Visible = true, Width = "Full" };

    private static FormSchemaDto SingleSectionSchema(params FormFieldDto[] fields) => new()
    {
        Sections = { new FormSectionDto { Key = "s1", Label = "Section 1", Fields = fields.ToList() } },
    };

    [Fact]
    public void Valid_Schema_Passes()
    {
        var schema = SingleSectionSchema(Field("a"), Field("b"));
        FormSchemaValidator.Validate(schema); // should not throw
    }

    [Fact]
    public void Rejects_Duplicate_Field_Keys()
    {
        var schema = SingleSectionSchema(Field("a"), Field("a"));
        Assert.Throws<FormsException>(() => FormSchemaValidator.Validate(schema));
    }

    [Fact]
    public void Rejects_Unknown_Field_Type()
    {
        var schema = SingleSectionSchema(Field("a", type: "NotARealType"));
        Assert.Throws<FormsException>(() => FormSchemaValidator.Validate(schema));
    }

    [Fact]
    public void Select_Field_Requires_At_Least_One_Option()
    {
        var schema = SingleSectionSchema(Field("a", type: FormFieldType.Select));
        Assert.Throws<FormsException>(() => FormSchemaValidator.Validate(schema));
    }

    [Fact]
    public void Rejects_Condition_Referencing_Unknown_Field()
    {
        var target = Field("target");
        target.ConditionCombinator = "AND";
        target.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "ghost", Operator = FormConditionOperator.EqualsOp, Value = "x" });

        var schema = SingleSectionSchema(target);
        Assert.Throws<FormsException>(() => FormSchemaValidator.Validate(schema));
    }

    [Fact]
    public void Rejects_Self_Referencing_Condition()
    {
        var target = Field("target");
        target.ConditionCombinator = "AND";
        target.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "target", Operator = FormConditionOperator.EqualsOp, Value = "x" });

        var schema = SingleSectionSchema(target);
        Assert.Throws<FormsException>(() => FormSchemaValidator.Validate(schema));
    }

    [Fact]
    public void Detects_Circular_Dependency_Between_Two_Fields()
    {
        var a = Field("a");
        a.ConditionCombinator = "AND";
        a.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "b", Operator = FormConditionOperator.EqualsOp, Value = "x" });

        var b = Field("b");
        b.ConditionCombinator = "AND";
        b.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "a", Operator = FormConditionOperator.EqualsOp, Value = "y" });

        var schema = SingleSectionSchema(a, b);
        Assert.Throws<FormsException>(() => FormSchemaValidator.Validate(schema));
    }

    [Fact]
    public void Detects_Longer_Circular_Chain()
    {
        var a = Field("a");
        a.ConditionCombinator = "AND";
        a.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "c", Operator = FormConditionOperator.EqualsOp, Value = "x" });

        var b = Field("b");
        b.ConditionCombinator = "AND";
        b.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "a", Operator = FormConditionOperator.EqualsOp, Value = "x" });

        var c = Field("c");
        c.ConditionCombinator = "AND";
        c.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "b", Operator = FormConditionOperator.EqualsOp, Value = "x" });

        var schema = SingleSectionSchema(a, b, c);
        Assert.Throws<FormsException>(() => FormSchemaValidator.Validate(schema));
    }

    [Fact]
    public void Allows_Diamond_Shaped_Non_Circular_Dependencies()
    {
        // d depends on b and c; b and c both depend on a. Not circular.
        var a = Field("a");
        var b = Field("b");
        b.ConditionCombinator = "AND";
        b.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "a", Operator = FormConditionOperator.EqualsOp, Value = "x" });
        var c = Field("c");
        c.ConditionCombinator = "AND";
        c.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "a", Operator = FormConditionOperator.EqualsOp, Value = "x" });
        var d = Field("d");
        d.ConditionCombinator = "OR";
        d.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "b", Operator = FormConditionOperator.EqualsOp, Value = "x" });
        d.Conditions.Add(new FormFieldConditionDto { SourceFieldKey = "c", Operator = FormConditionOperator.EqualsOp, Value = "x" });

        var schema = SingleSectionSchema(a, b, c, d);
        FormSchemaValidator.Validate(schema); // should not throw
    }
}
