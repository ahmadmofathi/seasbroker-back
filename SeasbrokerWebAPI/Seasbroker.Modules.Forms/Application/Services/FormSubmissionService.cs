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
        var evaluated = Evaluate(formKey, schema, rawValues, files);
        var visibleFields = evaluated.VisibleFields;
        var normalized = evaluated.Normalized;
        var systemValues = evaluated.SystemValues;

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

        var additionalInfo = evaluated.AdditionalInfo;

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
            TrackingNumber = requestedQuote.TrackingNumber,
        };
    }

    private const string NotFoundMessage = "We couldn't find a request with that tracking number and email address.";

    public async Task<RequestEditFormDto> LoadForEditAsync(string? number, string? email, CancellationToken cancellationToken = default)
    {
        var (quote, submission, version) = await FindEditableAsync(number, email, cancellationToken);
        var schema = FormMapper.ToSchemaDto(version, version.FormDefinition.Key);
        var fieldsByKey = schema.Sections
            .SelectMany(s => s.Fields)
            .ToDictionary(f => f.Key, StringComparer.OrdinalIgnoreCase);

        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var stored in submission.Values)
        {
            if (fieldsByKey.TryGetValue(stored.FieldKey, out var field))
            {
                values[field.Key] = ToEditableValue(field, stored.ValueText);
            }
        }

        var files = await _dbContext.FormSubmissionFiles
            .AsNoTracking()
            .Where(f => f.FormSubmissionId == submission.Id)
            .OrderBy(f => f.Created)
            .Select(f => new RequestEditFileDto { FieldKey = f.FieldKey, FileName = f.FileName, SizeBytes = f.SizeBytes })
            .ToListAsync(cancellationToken);

        return new RequestEditFormDto
        {
            TrackingNumber = quote.TrackingNumber,
            Schema = schema,
            Values = values,
            Files = files,
        };
    }

    public async Task UpdateAsync(
        string? number,
        string? email,
        Dictionary<string, JsonElement> rawValues,
        CancellationToken cancellationToken = default)
    {
        var (quote, submission, version) = await FindEditableAsync(number, email, cancellationToken);
        var formKey = version.FormDefinition.Key;
        var schema = FormMapper.ToSchemaDto(version, formKey);

        var fieldsWithFiles = (await _dbContext.FormSubmissionFiles
                .Where(f => f.FormSubmissionId == submission.Id)
                .Select(f => f.FieldKey)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Same checks as registering. The email can't be changed (it's what identifies the customer),
        // and the files already uploaded stay as they are.
        var evaluated = Evaluate(
            formKey,
            schema,
            rawValues,
            new FormFileCollection(),
            lockedEmail: quote.Customer.Email,
            existingFileFields: fieldsWithFiles);

        var system = evaluated.SystemValues;
        quote.CargoType = Truncate(system.GetValueOrDefault(FormsConstants.SystemFieldKeys.CargoType), 255);
        quote.Weight = double.TryParse(system.GetValueOrDefault(FormsConstants.SystemFieldKeys.Weight), NumberStyles.Any, CultureInfo.InvariantCulture, out var weight) ? weight : 0;
        quote.DeparturePort = Truncate(system.GetValueOrDefault(FormsConstants.SystemFieldKeys.DeparturePort), 255);
        quote.DepartureTime = Truncate(system.GetValueOrDefault(FormsConstants.SystemFieldKeys.DepartureTime), 100);
        quote.ArrivalPort = Truncate(system.GetValueOrDefault(FormsConstants.SystemFieldKeys.ArrivalPort), 255);
        quote.ArrivalTime = Truncate(system.GetValueOrDefault(FormsConstants.SystemFieldKeys.ArrivalTime), 100);
        quote.Dimensions = Truncate(system.GetValueOrDefault(FormsConstants.SystemFieldKeys.Dimensions), 255);
        quote.AdditionalInfo = Truncate(evaluated.AdditionalInfo, 2000);
        quote.CustomerEditedAt = DateTime.UtcNow;

        // The same person (same email): keep their name and phone current for the team too.
        var customer = quote.Customer;
        if (!string.IsNullOrWhiteSpace(system.GetValueOrDefault(FormsConstants.SystemFieldKeys.PhoneNumber)))
        {
            customer.PhoneNumber = system[FormsConstants.SystemFieldKeys.PhoneNumber]!;
        }

        if (!string.IsNullOrWhiteSpace(system.GetValueOrDefault(FormsConstants.SystemFieldKeys.FirstName)))
        {
            customer.FirstName = system[FormsConstants.SystemFieldKeys.FirstName]!;
        }

        if (!string.IsNullOrWhiteSpace(system.GetValueOrDefault(FormsConstants.SystemFieldKeys.LastName)))
        {
            customer.LastName = system[FormsConstants.SystemFieldKeys.LastName]!;
        }

        // Replace the stored answers with the new ones (answers to now-hidden fields are dropped, as when registering).
        _dbContext.FormSubmissionValues.RemoveRange(submission.Values);
        foreach (var field in evaluated.VisibleFields.Where(f => !FormFieldType.FileBased.Contains(f.Type)))
        {
            _dbContext.FormSubmissionValues.Add(new FormSubmissionValue
            {
                FormSubmissionId = submission.Id,
                FieldKey = field.Key,
                ValueText = evaluated.Normalized.GetValueOrDefault(field.Key),
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Finds the customer's request by tracking number + email and checks it can still be edited: it must
    /// have stored answers (sent through a form) and the team must not have accepted it yet - i.e. it hasn't
    /// become a cargo listing or a fleet vessel. A wrong number and a wrong email give the same answer.
    /// </summary>
    private async Task<(RequestedQuote Quote, FormSubmission Submission, FormVersion Version)> FindEditableAsync(
        string? number,
        string? email,
        CancellationToken cancellationToken)
    {
        var normalizedNumber = number?.Trim().ToUpperInvariant() ?? string.Empty;
        var normalizedEmail = email?.Trim() ?? string.Empty;
        if (normalizedNumber.Length == 0 || normalizedEmail.Length == 0)
        {
            throw new FormsException("Enter your tracking number and email address.", StatusCodes.Status400BadRequest);
        }

        var quote = await _dbContext.RequestedQuotes
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.TrackingNumber == normalizedNumber, cancellationToken);
        if (quote is null || !string.Equals(quote.Customer.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormsException(NotFoundMessage, StatusCodes.Status404NotFound);
        }

        var submission = await _dbContext.FormSubmissions
            .Include(s => s.Values)
            .FirstOrDefaultAsync(s => s.RequestedQuoteId == quote.Id, cancellationToken)
            ?? throw new FormsException(
                "This request was sent before online editing was available. Please contact us if you need to change it.",
                StatusCodes.Status409Conflict);

        var accepted =
            await _dbContext.CargoListings.AnyAsync(c => c.RequestedQuoteId == quote.Id, cancellationToken) ||
            await _dbContext.Vessels.AnyAsync(v => v.RequestedQuoteId == quote.Id, cancellationToken);
        if (accepted)
        {
            throw new FormsException(
                "Your request has already been accepted and is being processed, so it can't be edited here. Please contact us to change it.",
                StatusCodes.Status409Conflict);
        }

        var version = await _dbContext.FormVersions
            .AsNoTracking()
            .Include(v => v.FormDefinition)
            .Include(v => v.Sections).ThenInclude(s => s.Fields).ThenInclude(f => f.Options)
            .Include(v => v.Sections).ThenInclude(s => s.Fields).ThenInclude(f => f.Conditions)
            .FirstAsync(v => v.Id == submission.FormVersionId, cancellationToken);

        return (quote, submission, version);
    }

    /// <summary>Turns a stored answer back into the value its field's input works with.</summary>
    private static object? ToEditableValue(FormFieldDto field, string? text)
    {
        if (text is null)
        {
            return null;
        }

        if (field.Type == FormFieldType.MultiSelect)
        {
            return ParseJsonStringArray(text);
        }

        if (field.Type == FormFieldType.Route)
        {
            try
            {
                return JsonSerializer.Deserialize<JsonElement>(text);
            }
            catch (JsonException)
            {
                return Array.Empty<object>();
            }
        }

        if (field.Type == FormFieldType.Checkbox || field.Type == FormFieldType.Toggle)
        {
            return string.Equals(text, "true", StringComparison.OrdinalIgnoreCase);
        }

        return text;
    }

    private sealed record EvaluatedSubmission(
        List<FormFieldDto> VisibleFields,
        Dictionary<string, string?> Normalized,
        Dictionary<string, string?> SystemValues,
        string AdditionalInfo);

    /// <summary>
    /// Works out which fields are visible for the given answers, validates them, and derives the
    /// values the request is built from. Shared by registering a request and by a customer editing it.
    /// When editing, <paramref name="lockedEmail"/> keeps the customer's email whatever was sent, and
    /// <paramref name="existingFileFields"/> lists the file fields that already have uploaded files
    /// (files themselves aren't changed by an edit, so a required file field is satisfied by them).
    /// </summary>
    private static EvaluatedSubmission Evaluate(
        string formKey,
        FormSchemaDto schema,
        Dictionary<string, JsonElement> rawValues,
        IFormFileCollection files,
        string? lockedEmail = null,
        IReadOnlySet<string>? existingFileFields = null)
    {
        var allFields = schema.Sections.SelectMany(s => s.Fields).ToList();

        var normalized = allFields.ToDictionary(
            f => f.Key,
            f => NormalizeRawValue(rawValues.GetValueOrDefault(f.Key)),
            StringComparer.OrdinalIgnoreCase);

        if (lockedEmail is not null)
        {
            var emailField = allFields.FirstOrDefault(f => f.IsSystemField && f.SystemFieldKey == FormsConstants.SystemFieldKeys.Email);
            if (emailField is not null)
            {
                normalized[emailField.Key] = lockedEmail;
            }
        }

        var visibleFields = allFields.Where(f => ConditionEvaluator.IsVisible(f, normalized)).ToList();

        foreach (var field in visibleFields)
        {
            if (existingFileFields is not null && FormFieldType.FileBased.Contains(field.Type))
            {
                if (field.Required && !existingFileFields.Contains(field.Key))
                {
                    throw new FormsException($"'{field.Label}' is required.", StatusCodes.Status400BadRequest);
                }

                continue;
            }

            ValidateField(field, normalized.GetValueOrDefault(field.Key), files);
            ValidateAfterField(field, visibleFields, normalized);
        }

        var systemValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in visibleFields.Where(f => f.IsSystemField && !string.IsNullOrEmpty(f.SystemFieldKey)))
        {
            systemValues[field.SystemFieldKey!] = normalized.GetValueOrDefault(field.Key);
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

        return new EvaluatedSubmission(visibleFields, normalized, systemValues, additionalInfo);
    }

    private static void ValidateAfterField(FormFieldDto field, List<FormFieldDto> visibleFields, Dictionary<string, string?> values)
    {
        var afterKey = field.Validation?.AfterField;
        if (string.IsNullOrWhiteSpace(afterKey))
        {
            return;
        }

        var other = visibleFields.FirstOrDefault(f => string.Equals(f.Key, afterKey, StringComparison.OrdinalIgnoreCase));
        if (other is null ||
            !DateTime.TryParse(values.GetValueOrDefault(field.Key), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
            !DateTime.TryParse(values.GetValueOrDefault(other.Key), CultureInfo.InvariantCulture, DateTimeStyles.None, out var otherDate))
        {
            return;
        }

        if (date <= otherDate)
        {
            throw new FormsException($"'{field.Label}' must be after '{other.Label}'.", StatusCodes.Status400BadRequest);
        }
    }

    private sealed record RouteStopValue(
        [property: System.Text.Json.Serialization.JsonPropertyName("port")] string? Port,
        [property: System.Text.Json.Serialization.JsonPropertyName("eta")] string? Eta);

    private static List<RouteStopValue> ParseRoute(string? json)
    {
        try
        {
            return string.IsNullOrWhiteSpace(json)
                ? new List<RouteStopValue>()
                : JsonSerializer.Deserialize<List<RouteStopValue>>(json) ?? new List<RouteStopValue>();
        }
        catch (JsonException)
        {
            return new List<RouteStopValue> { new(null, null) };
        }
    }

    private static void ValidateRoute(FormFieldDto field, string? value)
    {
        var v = field.Validation;
        var stops = ParseRoute(value);

        if (v?.MinSelections is not null && stops.Count < v.MinSelections)
        {
            throw new FormsException($"'{field.Label}' needs at least {v.MinSelections} ports.", StatusCodes.Status400BadRequest);
        }

        var max = v?.MaxSelections ?? FormsConstants.MaxRouteStops;
        if (stops.Count > max)
        {
            throw new FormsException($"'{field.Label}' allows at most {max} ports.", StatusCodes.Status400BadRequest);
        }

        DateTime? previousEta = null;
        string? previousPort = null;
        for (var i = 0; i < stops.Count; i++)
        {
            var stopLabel = i == 0 ? "Next port" : $"Port {i + 1}";
            var port = stops[i].Port?.Trim();

            if (string.IsNullOrEmpty(port) || port.Length > 255)
            {
                throw new FormsException($"'{field.Label}': {stopLabel} needs a port.", StatusCodes.Status400BadRequest);
            }

            if (!DateTime.TryParse(stops[i].Eta, CultureInfo.InvariantCulture, DateTimeStyles.None, out var eta))
            {
                throw new FormsException($"'{field.Label}': {stopLabel} needs a valid ETA.", StatusCodes.Status400BadRequest);
            }

            if (v?.NoPastDates == true && eta.Date < DateTime.UtcNow.Date)
            {
                throw new FormsException($"'{field.Label}': {stopLabel} ETA cannot be in the past.", StatusCodes.Status400BadRequest);
            }

            if (string.Equals(port, previousPort, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormsException($"'{field.Label}': {stopLabel} repeats the port before it.", StatusCodes.Status400BadRequest);
            }

            if (previousEta is not null && eta <= previousEta)
            {
                throw new FormsException($"'{field.Label}': {stopLabel} ETA must be after the previous port's ETA.", StatusCodes.Status400BadRequest);
            }

            previousPort = port;
            previousEta = eta;
        }
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

                if (v?.WholeNumber == true && numeric != Math.Floor(numeric))
                {
                    throw new FormsException($"'{field.Label}' must be a whole number.", StatusCodes.Status400BadRequest);
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
                if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    throw new FormsException($"'{field.Label}' has an invalid date/time.", StatusCodes.Status400BadRequest);
                }

                if (v?.NoPastDates == true && t != FormFieldType.Time && parsedDate.Date < DateTime.UtcNow.Date)
                {
                    throw new FormsException($"'{field.Label}' cannot be a date in the past.", StatusCodes.Status400BadRequest);
                }

                break;

            case var t when t == FormFieldType.Route:
                ValidateRoute(field, value);
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
                // When FixedPrefix is set, MinLength/MaxLength describe the part typed after the
                // prefix (for the frontend's live character cap) rather than the full saved string,
                // so Pattern - matching the full string - is the source of truth here instead.
                if (string.IsNullOrEmpty(v?.FixedPrefix))
                {
                    if (v?.MinLength is not null && value!.Length < v.MinLength)
                    {
                        throw new FormsException($"'{field.Label}' must be at least {v.MinLength} characters.", StatusCodes.Status400BadRequest);
                    }

                    if (v?.MaxLength is not null && value!.Length > v.MaxLength)
                    {
                        throw new FormsException($"'{field.Label}' must be at most {v.MaxLength} characters.", StatusCodes.Status400BadRequest);
                    }
                }

                if (!string.IsNullOrEmpty(v?.Pattern) && !System.Text.RegularExpressions.Regex.IsMatch(value!, v.Pattern))
                {
                    throw new FormsException($"'{field.Label}' is not in a valid format.", StatusCodes.Status400BadRequest);
                }

                if (v?.NoFutureYear == true && int.TryParse(value, out var year) && year > DateTime.UtcNow.Year)
                {
                    throw new FormsException($"'{field.Label}' cannot be later than {DateTime.UtcNow.Year}.", StatusCodes.Status400BadRequest);
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

        if (field.Type == FormFieldType.Route && value is not null)
        {
            return string.Join(" → ", ParseRoute(value).Select(s => $"{s.Port} (ETA {s.Eta})"));
        }

        return value ?? string.Empty;
    }

    private static string Truncate(string? value, int maxLength)
    {
        value ??= string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
