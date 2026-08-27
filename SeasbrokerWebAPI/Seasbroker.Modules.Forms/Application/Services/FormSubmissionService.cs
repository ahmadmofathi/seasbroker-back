using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Forms.Application.Constants;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Exceptions;
using Seasbroker.Modules.Forms.Application.Mapping;

namespace Seasbroker.Modules.Forms.Application.Services;

public class FormSubmissionService : IFormSubmissionService
{
    private static readonly Dictionary<string, string> ServiceTags = new()
    {
        [FormsConstants.FormKeys.RequestQuote] = "Cargo Brokerage",
        [FormsConstants.FormKeys.RequestRoute] = "Ship Brokerage",
        [FormsConstants.FormKeys.RequestClearance] = "Customs Clearance",
    };

    private readonly SeasbrokerDbContext _dbContext;
    private readonly IFileStorageService _fileStorage;

    public FormSubmissionService(SeasbrokerDbContext dbContext, IFileStorageService fileStorage)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
    }

    public async Task<SubmitFormResponse> SubmitAsync(
        string formKey,
        Dictionary<string, JsonElement> rawValues,
        IFormFileCollection files,
        CancellationToken cancellationToken = default)
    {
        var definition = await _dbContext.FormDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Key == formKey, cancellationToken)
            ?? throw new FormsException($"Unknown form '{formKey}'.", StatusCodes.Status404NotFound);

        var version = await _dbContext.FormVersions
            .AsNoTracking()
            .Include(v => v.Sections).ThenInclude(s => s.Fields).ThenInclude(f => f.Options)
            .Include(v => v.Sections).ThenInclude(s => s.Fields).ThenInclude(f => f.Conditions)
            .Where(v => v.FormDefinitionId == definition.Id && v.Status == FormVersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new FormsException($"Form '{formKey}' is not currently accepting submissions.", StatusCodes.Status409Conflict);

        var schema = FormMapper.ToSchemaDto(version, formKey);
        var allFields = schema.Sections.SelectMany(s => s.Fields).ToList();

        var normalized = allFields.ToDictionary(
            f => f.Key,
            f => NormalizeRawValue(rawValues.GetValueOrDefault(f.Key)),
            StringComparer.OrdinalIgnoreCase);

        var visibleFields = allFields.Where(f => ConditionEvaluator.IsVisible(f, normalized)).ToList();

        foreach (var field in visibleFields)
        {
            ValidateField(field, normalized.GetValueOrDefault(field.Key), files);
        }

        var systemValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in visibleFields.Where(f => f.IsSystemField && !string.IsNullOrEmpty(f.SystemFieldKey)))
        {
            systemValues[field.SystemFieldKey!] = normalized.GetValueOrDefault(field.Key);
        }

        var email = systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.Email)?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new FormsException("An email address is required to submit this form.", StatusCodes.Status400BadRequest);
        }

        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
        if (customer is null)
        {
            customer = new Customer
            {
                Email = email,
                PhoneNumber = systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.PhoneNumber) ?? string.Empty,
                FirstName = systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.FirstName) ?? string.Empty,
                LastName = systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.LastName) ?? string.Empty,
            };
            _dbContext.Customers.Add(customer);
        }

        var extraFields = visibleFields
            .Where(f => !FormFieldType.FileBased.Contains(f.Type))
            .Where(f => !(f.IsSystemField && f.SystemFieldKey is not null && FormsConstants.SystemFieldKeys.MappedToRequestedQuote.Contains(f.SystemFieldKey)))
            .ToList();

        var additionalInfo = BuildAdditionalInfo(
            ServiceTags.GetValueOrDefault(formKey, formKey),
            systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.AdditionalInfo),
            extraFields,
            normalized);

        var requestedQuote = new RequestedQuote
        {
            CustomerId = customer.Id,
            CargoType = Truncate(systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.CargoType), 255),
            Weight = double.TryParse(systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.Weight), NumberStyles.Any, CultureInfo.InvariantCulture, out var weight) ? weight : 0,
            DeparturePort = Truncate(systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.DeparturePort), 255),
            DepartureTime = Truncate(systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.DepartureTime), 100),
            ArrivalPort = Truncate(systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.ArrivalPort), 255),
            ArrivalTime = Truncate(systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.ArrivalTime), 100),
            Dimensions = Truncate(systemValues.GetValueOrDefault(FormsConstants.SystemFieldKeys.Dimensions), 255),
            AdditionalInfo = Truncate(additionalInfo, 2000),
        };
        _dbContext.RequestedQuotes.Add(requestedQuote);

        var submission = new FormSubmission
        {
            FormVersionId = version.Id,
            CustomerId = customer.Id,
            RequestedQuoteId = requestedQuote.Id,
        };
        _dbContext.FormSubmissions.Add(submission);

        foreach (var field in visibleFields.Where(f => !FormFieldType.FileBased.Contains(f.Type)))
        {
            _dbContext.FormSubmissionValues.Add(new FormSubmissionValue
            {
                FormSubmissionId = submission.Id,
                FieldKey = field.Key,
                ValueText = normalized.GetValueOrDefault(field.Key),
            });
        }

        foreach (var field in visibleFields.Where(f => FormFieldType.FileBased.Contains(f.Type)))
        {
            foreach (var file in GetFilesForField(files, field.Key))
            {
                var storagePath = await _fileStorage.SaveAsync(file, $"{formKey}/{submission.Id:N}", cancellationToken);
                _dbContext.FormSubmissionFiles.Add(new FormSubmissionFile
                {
                    FormSubmissionId = submission.Id,
                    FieldKey = field.Key,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    SizeBytes = file.Length,
                    StoragePath = storagePath,
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SubmitFormResponse
        {
            SubmissionId = submission.Id.ToString(),
            RequestedQuoteId = requestedQuote.Id.ToString(),
        };
    }

    private static void ValidateField(FormFieldDto field, string? value, IFormFileCollection files)
    {
        var isFile = FormFieldType.FileBased.Contains(field.Type);
        var uploadedFiles = isFile ? GetFilesForField(files, field.Key) : new List<IFormFile>();
        var isEmpty = isFile ? uploadedFiles.Count == 0 : string.IsNullOrWhiteSpace(value) || value == "[]";

        if (field.Required && isEmpty)
        {
            throw new FormsException($"'{field.Label}' is required.", StatusCodes.Status400BadRequest);
        }

        if (isEmpty)
        {
            return;
        }

        var v = field.Validation;

        switch (field.Type)
        {
            case var t when t == FormFieldType.Select || t == FormFieldType.Radio:
                if (!field.Options.Any(o => o.Value == value))
                {
                    throw new FormsException($"'{field.Label}' has an invalid selection.", StatusCodes.Status400BadRequest);
                }

                break;

            case var t when t == FormFieldType.MultiSelect:
                var selected = ParseJsonStringArray(value);
                if (selected.Any(s => !field.Options.Any(o => o.Value == s)))
                {
                    throw new FormsException($"'{field.Label}' has an invalid selection.", StatusCodes.Status400BadRequest);
                }

                if (v?.MinSelections is not null && selected.Count < v.MinSelections)
                {
                    throw new FormsException($"'{field.Label}' needs at least {v.MinSelections} selection(s).", StatusCodes.Status400BadRequest);
                }

                if (v?.MaxSelections is not null && selected.Count > v.MaxSelections)
                {
                    throw new FormsException($"'{field.Label}' allows at most {v.MaxSelections} selection(s).", StatusCodes.Status400BadRequest);
                }

                break;

            case var t when t == FormFieldType.Number || t == FormFieldType.Decimal:
                if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var numeric))
                {
                    throw new FormsException($"'{field.Label}' must be a number.", StatusCodes.Status400BadRequest);
                }

                if (v?.Min is not null && numeric < v.Min)
                {
                    throw new FormsException($"'{field.Label}' must be at least {v.Min}.", StatusCodes.Status400BadRequest);
                }

                if (v?.Max is not null && numeric > v.Max)
                {
                    throw new FormsException($"'{field.Label}' must be at most {v.Max}.", StatusCodes.Status400BadRequest);
                }

                break;

            case var t when t == FormFieldType.Date || t == FormFieldType.DateTime || t == FormFieldType.Time:
                if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    throw new FormsException($"'{field.Label}' has an invalid date/time.", StatusCodes.Status400BadRequest);
                }

                break;

            case var t when t == FormFieldType.File || t == FormFieldType.MultiFile:
                foreach (var file in uploadedFiles)
                {
                    if (v?.FileMaxSizeMB is not null && file.Length > v.FileMaxSizeMB * 1024 * 1024)
                    {
                        throw new FormsException($"'{field.Label}': file '{file.FileName}' exceeds the {v.FileMaxSizeMB} MB limit.", StatusCodes.Status400BadRequest);
                    }

                    if (file.Length > FormsConstants.MaxFileSizeBytesHardCap)
                    {
                        throw new FormsException($"'{field.Label}': file '{file.FileName}' is too large.", StatusCodes.Status400BadRequest);
                    }

                    if (v?.AllowedExtensions is { Count: > 0 })
                    {
                        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
                        if (!v.AllowedExtensions.Any(a => a.TrimStart('.').ToLowerInvariant() == ext))
                        {
                            throw new FormsException($"'{field.Label}': file type '.{ext}' is not allowed.", StatusCodes.Status400BadRequest);
                        }
                    }
                }

                break;

            default:
                if (v?.MinLength is not null && value!.Length < v.MinLength)
                {
                    throw new FormsException($"'{field.Label}' must be at least {v.MinLength} characters.", StatusCodes.Status400BadRequest);
                }

                if (v?.MaxLength is not null && value!.Length > v.MaxLength)
                {
                    throw new FormsException($"'{field.Label}' must be at most {v.MaxLength} characters.", StatusCodes.Status400BadRequest);
                }

                if (!string.IsNullOrEmpty(v?.Pattern) && !System.Text.RegularExpressions.Regex.IsMatch(value!, v.Pattern))
                {
                    throw new FormsException($"'{field.Label}' is not in a valid format.", StatusCodes.Status400BadRequest);
                }

                break;
        }
    }

    private static List<IFormFile> GetFilesForField(IFormFileCollection files, string fieldKey) =>
        files.Where(f => f.Name == $"file:{fieldKey}").ToList();

    private static string? NormalizeRawValue(JsonElement? element)
    {
        if (element is null)
        {
            return null;
        }

        var el = element.Value;
        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Array => el.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => el.GetRawText(),
            JsonValueKind.Null => null,
            _ => null,
        };
    }

    private static List<string> ParseJsonStringArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static string BuildAdditionalInfo(
        string tag,
        string? remarks,
        List<FormFieldDto> extraFields,
        Dictionary<string, string?> normalized)
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(remarks))
        {
            lines.Add(remarks.Trim());
        }

        var detailLines = extraFields
            .Select(f => (Field: f, Value: normalized.GetValueOrDefault(f.Key)))
            .Where(x => !string.IsNullOrWhiteSpace(x.Value) && x.Value != "[]")
            .Select(x => $"{x.Field.Label}: {DisplayValue(x.Field, x.Value)}")
            .ToList();

        if (detailLines.Count > 0)
        {
            lines.Add("=== Additional Details ===");
            lines.AddRange(detailLines);
        }

        var body = string.Join("\n", lines);
        return body.Length > 0 ? $"[{tag}] {body}" : $"[{tag}]";
    }

    private static string DisplayValue(FormFieldDto field, string? value)
    {
        if (field.Type == FormFieldType.MultiSelect && value is not null)
        {
            return string.Join(", ", ParseJsonStringArray(value));
        }

        return value ?? string.Empty;
    }

    private static string Truncate(string? value, int maxLength)
    {
        value ??= string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
