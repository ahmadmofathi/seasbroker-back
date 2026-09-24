using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.Constants;
using Seasbroker.Modules.Forms.Application.DTOs;

namespace Seasbroker.Modules.Forms.Infrastructure;

/// <summary>
/// Targeted, idempotent edits that bring an already-live form up to date with changes made to
/// <see cref="FormSeedData"/> after it was first seeded. Unlike re-seeding, these patch the form
/// as it currently is - everything an admin changed in the Form Builder is kept. Each patch only
/// touches its own field(s), and skips them if an admin has removed them or already changed them
/// in a way the patch doesn't recognise.
/// </summary>
public static class FormSchemaUpgrades
{
    /// <summary>Applies every pending patch for <paramref name="formKey"/>. Returns true if anything changed.</summary>
    public static bool Apply(string formKey, FormSchemaDto schema)
    {
        var changed = false;

        if (formKey == FormsConstants.FormKeys.RequestQuote)
        {
            changed |= RejectPastCargoDates(schema);
            changed |= ArrivalAfterCargoReady(schema);
            changed |= AllowMultipleFiles(schema, "documentsFile", "projDrawingsFile", "otherDocFile");
            changed |= GasIsAlwaysDangerous(schema);
        }
        else if (formKey == FormsConstants.FormKeys.RequestRoute)
        {
            changed |= ScheduledRouteBuilder(schema);
        }
        else if (formKey == FormsConstants.FormKeys.RequestClearance)
        {
            changed |= AllowMultipleFiles(schema, "otherSupportingDocs");
        }

        return changed;
    }

    private static FormFieldDto? Find(FormSchemaDto schema, string key) =>
        schema.Sections.SelectMany(s => s.Fields).FirstOrDefault(f => string.Equals(f.Key, key, StringComparison.OrdinalIgnoreCase));

    private static bool RejectPastCargoDates(FormSchemaDto schema)
    {
        var changed = false;
        foreach (var key in new[] { "cargoReadyDate", "estimatedArrivalDate" })
        {
            var field = Find(schema, key);
            if (field is null || field.Validation?.NoPastDates == true)
            {
                continue;
            }

            field.Validation ??= new FormFieldValidationDto();
            field.Validation.NoPastDates = true;
            changed = true;
        }

        return changed;
    }

    private static bool ArrivalAfterCargoReady(FormSchemaDto schema)
    {
        var arrival = Find(schema, "estimatedArrivalDate");
        var ready = Find(schema, "cargoReadyDate");
        if (arrival is null || ready is null || !string.IsNullOrEmpty(arrival.Validation?.AfterField))
        {
            return false;
        }

        arrival.Validation ??= new FormFieldValidationDto();
        arrival.Validation.AfterField = ready.Key;
        return true;
    }

    private static bool AllowMultipleFiles(FormSchemaDto schema, params string[] keys)
    {
        var changed = false;
        foreach (var key in keys)
        {
            var field = Find(schema, key);
            if (field?.Type != FormFieldType.File)
            {
                continue;
            }

            field.Type = FormFieldType.MultiFile;
            changed = true;
        }

        return changed;
    }

    /// <summary>Hide "Dangerous Goods?" for Gas cargo and always show the DG detail fields for it.</summary>
    private static bool GasIsAlwaysDangerous(FormSchemaDto schema)
    {
        var changed = false;

        var question = Find(schema, "dangerousGoods");
        if (question is not null && question.Conditions.Count == 0)
        {
            question.ConditionCombinator = FormConditionCombinator.And;
            question.Conditions.Add(new FormFieldConditionDto
            {
                SourceFieldKey = "cargoType",
                Operator = FormConditionOperator.NotEquals,
                Value = "Gas",
            });
            changed = true;
        }

        var detailFields = schema.Sections
            .Where(s => s.Key == "dangerous-goods")
            .SelectMany(s => s.Fields);

        foreach (var field in detailFields)
        {
            // Only the untouched seed shape: a single "dangerousGoods is Yes" condition.
            if (field.Conditions.Count != 1 ||
                field.Conditions[0] is not { SourceFieldKey: "dangerousGoods", Operator: FormConditionOperator.EqualsOp, Value: "Yes" })
            {
                continue;
            }

            field.ConditionCombinator = FormConditionCombinator.Or;
            field.Conditions.Add(new FormFieldConditionDto
            {
                SourceFieldKey = "cargoType",
                Operator = FormConditionOperator.EqualsOp,
                Value = "Gas",
            });
            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// Replaces the Scheduled Route branch's separate Departure Port / Date, Destination Port, ETA
    /// and free-text Transit Ports with a single Route field (next port first, then "Add port").
    /// </summary>
    private static bool ScheduledRouteBuilder(FormSchemaDto schema)
    {
        var oldKeys = new[] { "schedDeparturePort", "schedDepartureDate", "schedDestinationPort", "schedEta", "transitPorts" };

        if (Find(schema, "schedRoute") is not null)
        {
            return false;
        }

        var section = schema.Sections.FirstOrDefault(s => s.Fields.Any(f => f.Key == "schedDeparturePort"));
        if (section is null)
        {
            return false;
        }

        // Don't remove fields that something else still depends on.
        var otherFields = schema.Sections.SelectMany(s => s.Fields).Where(f => !oldKeys.Contains(f.Key));
        if (otherFields.Any(f => f.Conditions.Any(c => oldKeys.Contains(c.SourceFieldKey)) ||
                                 oldKeys.Contains(f.Validation?.AfterField ?? string.Empty)))
        {
            return false;
        }

        var anchor = section.Fields.First(f => f.Key == "schedDeparturePort");
        var route = new FormFieldDto
        {
            Key = "schedRoute",
            Label = "Route",
            Type = FormFieldType.Route,
            Required = true,
            Visible = true,
            Order = anchor.Order,
            Width = FormFieldWidth.Full,
            ConditionCombinator = anchor.ConditionCombinator,
            Conditions = anchor.Conditions
                .Select(c => new FormFieldConditionDto { SourceFieldKey = c.SourceFieldKey, Operator = c.Operator, Value = c.Value })
                .ToList(),
            Validation = new FormFieldValidationDto { NoPastDates = true, MinSelections = 2 },
        };

        foreach (var s in schema.Sections)
        {
            s.Fields.RemoveAll(f => oldKeys.Contains(f.Key));
        }

        section.Fields.Add(route);
        section.Fields = section.Fields.OrderBy(f => f.Order).ToList();
        return true;
    }
}
