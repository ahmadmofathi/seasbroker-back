using Microsoft.AspNetCore.Http;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Exceptions;

namespace Seasbroker.Modules.Forms.Application.Services;

/// <summary>
/// Validates an admin-authored draft schema structurally before it is persisted. Every failure
/// is deterministic, config-driven data validation - never arbitrary code execution.
/// </summary>
public static class FormSchemaValidator
{
    public static void Validate(FormSchemaDto schema)
    {
        if (schema.Sections.Count == 0)
        {
            Fail("A form must have at least one section.");
        }

        var sectionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var allFields = new Dictionary<string, FormFieldDto>(StringComparer.OrdinalIgnoreCase);

        foreach (var section in schema.Sections)
        {
            if (string.IsNullOrWhiteSpace(section.Key))
            {
                Fail("Every section needs a key.");
            }

            if (!sectionKeys.Add(section.Key))
            {
                Fail($"Duplicate section key '{section.Key}'.");
            }

            foreach (var field in section.Fields)
            {
                ValidateFieldShape(field);

                if (!allFields.TryAdd(field.Key, field))
                {
                    Fail($"Duplicate field key '{field.Key}'.");
                }
            }
        }

        if (allFields.Count == 0)
        {
            Fail("A form must have at least one field.");
        }

        foreach (var field in allFields.Values)
        {
            ValidateConditions(field, allFields);
        }

        DetectCircularDependencies(allFields);
    }

    private static void ValidateFieldShape(FormFieldDto field)
    {
        if (string.IsNullOrWhiteSpace(field.Key) || !System.Text.RegularExpressions.Regex.IsMatch(field.Key, "^[a-zA-Z0-9_-]+$"))
        {
            Fail($"Field key '{field.Key}' must be non-empty and contain only letters, numbers, '-' and '_'.");
        }

        if (string.IsNullOrWhiteSpace(field.Label))
        {
            Fail($"Field '{field.Key}' needs a label.");
        }

        if (!FormFieldType.All.Contains(field.Type))
        {
            Fail($"Field '{field.Key}' has an unknown type '{field.Type}'.");
        }

        if (!FormFieldWidth.All.Contains(field.Width))
        {
            Fail($"Field '{field.Key}' has an unknown width '{field.Width}'.");
        }

        if (FormFieldType.OptionBased.Contains(field.Type) && field.Options.Count == 0)
        {
            Fail($"Field '{field.Key}' is a {field.Type} field and needs at least one option.");
        }

        var optionValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var option in field.Options)
        {
            if (string.IsNullOrWhiteSpace(option.Value))
            {
                Fail($"Field '{field.Key}' has an option with no value.");
            }

            if (!optionValues.Add(option.Value))
            {
                Fail($"Field '{field.Key}' has duplicate option value '{option.Value}'.");
            }
        }

        if (field.IsSystemField && string.IsNullOrWhiteSpace(field.SystemFieldKey))
        {
            Fail($"System field '{field.Key}' is missing its SystemFieldKey.");
        }

        if (field.Conditions.Count > 0 && !FormConditionCombinator.All.Contains(field.ConditionCombinator ?? string.Empty))
        {
            Fail($"Field '{field.Key}' has conditions but an invalid combinator.");
        }
    }

    private static void ValidateConditions(FormFieldDto field, Dictionary<string, FormFieldDto> allFields)
    {
        foreach (var condition in field.Conditions)
        {
            if (!FormConditionOperator.All.Contains(condition.Operator))
            {
                Fail($"Field '{field.Key}' has an unknown condition operator '{condition.Operator}'.");
            }

            if (string.IsNullOrWhiteSpace(condition.SourceFieldKey))
            {
                Fail($"Field '{field.Key}' has a condition with no source field.");
            }

            if (string.Equals(condition.SourceFieldKey, field.Key, StringComparison.OrdinalIgnoreCase))
            {
                Fail($"Field '{field.Key}' cannot depend on itself.");
            }

            if (!allFields.ContainsKey(condition.SourceFieldKey))
            {
                Fail($"Field '{field.Key}' has a condition referencing unknown field '{condition.SourceFieldKey}'.");
            }
        }
    }

    private static void DetectCircularDependencies(Dictionary<string, FormFieldDto> allFields)
    {
        var state = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // 0=unvisited,1=visiting,2=done

        foreach (var key in allFields.Keys)
        {
            if (!state.TryGetValue(key, out var s) || s == 0)
            {
                Visit(key, allFields, state);
            }
        }
    }

    private static void Visit(string key, Dictionary<string, FormFieldDto> allFields, Dictionary<string, int> state)
    {
        state[key] = 1;

        if (allFields.TryGetValue(key, out var field))
        {
            foreach (var dependsOnKey in field.Conditions.Select(c => c.SourceFieldKey).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!state.TryGetValue(dependsOnKey, out var depState) || depState == 0)
                {
                    Visit(dependsOnKey, allFields, state);
                }
                else if (depState == 1)
                {
                    Fail($"Circular conditional dependency detected involving field '{dependsOnKey}'.");
                }
            }
        }

        state[key] = 2;
    }

    private static void Fail(string message) =>
        throw new FormsException(message, StatusCodes.Status400BadRequest);
}
