using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Exceptions;
using Seasbroker.Modules.Forms.Application.Services;

namespace Seasbroker.Modules.Forms.Controllers;

/// <summary>
/// Public-facing form API: fetch the currently published schema and submit against it.
/// Same schema shape the admin builder previews - one renderer, one contract.
/// </summary>
[ApiController]
[AllowAnonymous]
[Tags("Forms")]
[Route("api/forms")]
public class FormsPublicController : ControllerBase
{
    private readonly IFormBuilderService _formBuilderService;
    private readonly IFormSubmissionService _submissionService;

    public FormsPublicController(IFormBuilderService formBuilderService, IFormSubmissionService submissionService)
    {
        _formBuilderService = formBuilderService;
        _submissionService = submissionService;
    }

    [HttpGet("{key}/schema")]
    [ProducesResponseType(typeof(FormSchemaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchema(string key, CancellationToken cancellationToken)
    {
        var schema = await _formBuilderService.GetPublishedSchemaAsync(key, cancellationToken);
        if (schema is null)
        {
            return NotFound(new PocketBaseErrorResponse
            {
                Message = $"Form '{key}' is not available.",
                Status = StatusCodes.Status404NotFound,
            });
        }

        return Ok(schema);
    }

    /// <summary>
    /// multipart/form-data: a "payload" text part (JSON object of field values) plus file parts
    /// named "file:{fieldKey}" (repeat the name for MultiFile fields).
    /// </summary>
    [HttpPost("{key}/submissions")]
    [ProducesResponseType(typeof(SubmitFormResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Submit(string key, CancellationToken cancellationToken)
    {
        if (!Request.HasFormContentType)
        {
            throw new FormsException("Expected multipart/form-data.", StatusCodes.Status400BadRequest);
        }

        var form = await Request.ReadFormAsync(cancellationToken);
        var payloadJson = form["payload"].ToString();

        Dictionary<string, JsonElement> values;
        try
        {
            values = string.IsNullOrWhiteSpace(payloadJson)
                ? new Dictionary<string, JsonElement>()
                : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson) ?? new Dictionary<string, JsonElement>();
        }
        catch (JsonException)
        {
            throw new FormsException("Malformed submission payload.", StatusCodes.Status400BadRequest);
        }

        var response = await _submissionService.SubmitAsync(key, values, form.Files, cancellationToken);
        return Ok(response);
    }
}
