namespace Seasbroker.Infrastructure.Persistence.Entities;

/// <summary>
/// One condition contributing to the visibility rule of <see cref="FormFieldId"/>.
/// All of a field's conditions combine via that field's ConditionCombinator (AND/OR).
/// </summary>
public class FormFieldCondition : AuditableEntity
{
    public Guid FormFieldId { get; set; }

    public FormField FormField { get; set; } = null!;

    /// <summary>Key of the field whose submitted value is inspected.</summary>
    public string SourceFieldKey { get; set; } = string.Empty;

    public string Operator { get; set; } = FormConditionOperator.EqualsOp;

    /// <summary>Comparison value. For In/NotIn, a JSON array string. Unused for IsEmpty/IsNotEmpty.</summary>
    public string? Value { get; set; }

    public int Order { get; set; }
}
