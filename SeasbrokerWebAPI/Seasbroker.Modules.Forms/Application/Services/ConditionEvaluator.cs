using System.Text.Json;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.DTOs;

namespace Seasbroker.Modules.Forms.Application.Services;

/// <summary>
/// Evaluates a field's visibility from the values submitted for the fields it depends on.
/// Mirrors the frontend engine exactly (same operator semantics) so client and server agree.
/// </summary>
public static class ConditionEvaluator
{
    public static bool IsVisible(FormFieldDto field, IReadOnlyDictionary<string, string?> submittedValues)
    {
        if (!field.Visible)
        {
            return false;
        }

        if (field.Conditions.Count == 0)
        {
            return true;
        }

        var results = field.Conditions.Select(c => EvaluateCondition(c, submittedValues));

        return field.ConditionCombinator == FormConditionCombinator.Or
            ? results.Any(r => r)
            : results.All(r => r);
    }

    private static bool EvaluateCondition(FormFieldConditionDto condition, IReadOnlyDictionary<string, string?> values)
    {
        values.TryGetValue(condition.SourceFieldKey, out var raw);
        var actual = raw?.Trim();

        switch (condition.Operator)
        {
            case FormConditionOperator.IsEmpty:
                return string.IsNullOrEmpty(actual) || actual == "[]";
            case FormConditionOperator.IsNotEmpty:
                return !string.IsNullOrEmpty(actual) && actual != "[]";
            case FormConditionOperator.EqualsOp:
                return string.Equals(actual, condition.Value, StringComparison.OrdinalIgnoreCase);
            case FormConditionOperator.NotEquals:
                return !string.Equals(actual, condition.Value, StringComparison.OrdinalIgnoreCase);
            case FormConditionOperator.Contains:
                return ToList(actual).Any(v => string.Equals(v, condition.Value, StringComparison.OrdinalIgnoreCase))
                    || (actual?.Contains(condition.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase) ?? false);
            case FormConditionOperator.In:
                return ToList(condition.Value).Any(v => string.Equals(v, actual, StringComparison.OrdinalIgnoreCase));
            case FormConditionOperator.NotIn:
                return !ToList(condition.Value).Any(v => string.Equals(v, actual, StringComparison.OrdinalIgnoreCase));
            case FormConditionOperator.GreaterThan:
                return Compare(actual, condition.Value) > 0;
            case FormConditionOperator.GreaterThanOrEqual:
                return Compare(actual, condition.Value) >= 0;
            case FormConditionOperator.LessThan:
                return Compare(actual, condition.Value) < 0;
            case FormConditionOperator.LessThanOrEqual:
                return Compare(actual, condition.Value) <= 0;
            default:
                return false;
        }
    }

    private static int Compare(string? actual, string? expected)
    {
        if (double.TryParse(actual, out var a) && double.TryParse(expected, out var b))
        {
            return a.CompareTo(b);
        }

        if (DateTime.TryParse(actual, out var da) && DateTime.TryParse(expected, out var db))
        {
            return da.CompareTo(db);
        }

        return string.CompareOrdinal(actual, expected);
    }

    private static List<string> ToList(string? jsonArrayOrScalar)
    {
        if (string.IsNullOrWhiteSpace(jsonArrayOrScalar))
        {
            return new List<string>();
        }

        if (jsonArrayOrScalar.TrimStart().StartsWith('['))
        {
            try
            {
                return JsonSerializer.Deserialize<List<string>>(jsonArrayOrScalar) ?? new List<string>();
            }
            catch (JsonException)
            {
                return new List<string>();
            }
        }

        return jsonArrayOrScalar.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0).ToList();
    }
}
