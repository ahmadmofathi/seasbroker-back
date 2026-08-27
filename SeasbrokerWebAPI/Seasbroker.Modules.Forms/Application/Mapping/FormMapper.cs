using System.Text.Json;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.DTOs;

namespace Seasbroker.Modules.Forms.Application.Mapping;

public static class FormMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static FormSchemaDto ToSchemaDto(FormVersion version, string formKey)
    {
        return new FormSchemaDto
        {
            FormKey = formKey,
            VersionNumber = version.VersionNumber,
            Status = version.Status,
            Sections = version.Sections
                .OrderBy(s => s.Order)
                .Select(ToSectionDto)
                .ToList(),
        };
    }

    public static FormSectionDto ToSectionDto(FormSection section)
    {
        return new FormSectionDto
        {
            Key = section.Key,
            Label = section.Label,
            Order = section.Order,
            Visible = section.Visible,
            Fields = section.Fields.OrderBy(f => f.Order).Select(ToFieldDto).ToList(),
        };
    }

    public static FormFieldDto ToFieldDto(FormField field)
    {
        return new FormFieldDto
        {
            Key = field.Key,
            Label = field.Label,
            Type = field.Type,
            Placeholder = field.Placeholder,
            HelpText = field.HelpText,
            Required = field.Required,
            Visible = field.Visible,
            Order = field.Order,
            Width = field.Width,
            IsSystemField = field.IsSystemField,
            SystemFieldKey = field.SystemFieldKey,
            DefaultValue = field.DefaultValue,
            Options = field.Options.OrderBy(o => o.Order)
                .Select(o => new FormFieldOptionDto { Value = o.Value, Label = o.Label, Order = o.Order })
                .ToList(),
            Validation = string.IsNullOrWhiteSpace(field.ValidationJson)
                ? null
                : JsonSerializer.Deserialize<FormFieldValidationDto>(field.ValidationJson, JsonOptions),
            ConditionCombinator = field.ConditionCombinator,
            Conditions = field.Conditions.OrderBy(c => c.Order)
                .Select(c => new FormFieldConditionDto { SourceFieldKey = c.SourceFieldKey, Operator = c.Operator, Value = c.Value })
                .ToList(),
        };
    }

    /// <summary>Builds a brand new (unsaved) FormVersion entity tree from a schema DTO.</summary>
    public static FormVersion ToNewVersion(Guid formDefinitionId, int versionNumber, string status, FormSchemaDto dto)
    {
        var version = new FormVersion
        {
            Id = Guid.NewGuid(),
            FormDefinitionId = formDefinitionId,
            VersionNumber = versionNumber,
            Status = status,
        };

        foreach (var sectionDto in dto.Sections)
        {
            var section = new FormSection
            {
                Id = Guid.NewGuid(),
                FormVersionId = version.Id,
                Key = sectionDto.Key,
                Label = sectionDto.Label,
                Order = sectionDto.Order,
                Visible = sectionDto.Visible,
            };

            foreach (var fieldDto in sectionDto.Fields)
            {
                var field = new FormField
                {
                    Id = Guid.NewGuid(),
                    FormVersionId = version.Id,
                    FormSectionId = section.Id,
                    Key = fieldDto.Key,
                    Label = fieldDto.Label,
                    Type = fieldDto.Type,
                    Placeholder = fieldDto.Placeholder,
                    HelpText = fieldDto.HelpText,
                    Required = fieldDto.Required,
                    Visible = fieldDto.Visible,
                    Order = fieldDto.Order,
                    Width = fieldDto.Width,
                    IsSystemField = fieldDto.IsSystemField,
                    SystemFieldKey = fieldDto.SystemFieldKey,
                    DefaultValue = fieldDto.DefaultValue,
                    ValidationJson = fieldDto.Validation is null ? null : JsonSerializer.Serialize(fieldDto.Validation, JsonOptions),
                    ConditionCombinator = fieldDto.Conditions.Count > 0 ? (fieldDto.ConditionCombinator ?? FormConditionCombinator.And) : null,
                };

                field.Options = fieldDto.Options
                    .Select(o => new FormFieldOption { Id = Guid.NewGuid(), FormFieldId = field.Id, Value = o.Value, Label = o.Label, Order = o.Order })
                    .ToList();

                field.Conditions = fieldDto.Conditions
                    .Select((c, i) => new FormFieldCondition { Id = Guid.NewGuid(), FormFieldId = field.Id, SourceFieldKey = c.SourceFieldKey, Operator = c.Operator, Value = c.Value, Order = i })
                    .ToList();

                section.Fields.Add(field);
            }

            version.Sections.Add(section);
        }

        return version;
    }
}
