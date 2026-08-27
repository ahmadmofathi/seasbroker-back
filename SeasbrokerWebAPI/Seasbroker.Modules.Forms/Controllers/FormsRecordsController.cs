using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Forms.Application.Constants;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Services;

namespace Seasbroker.Modules.Forms.Controllers;

/// <summary>
/// Admin form-builder API: list forms, load/save the draft version, publish it.
/// The currently published version is never mutated by these endpoints.
/// </summary>
[ApiController]
[Authorize(Policy = FormsConstants.SuperuserPolicy)]
[Tags("Forms")]
[Route("api/collections/forms/records")]
public class FormsRecordsController : ControllerBase
{
    private readonly IFormBuilderService _formBuilderService;

    public FormsRecordsController(IFormBuilderService formBuilderService)
    {
        _formBuilderService = formBuilderService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FormSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        return Ok(await _formBuilderService.ListFormsAsync(cancellationToken));
    }

    [HttpGet("{key}/draft")]
    [ProducesResponseType(typeof(FormSchemaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDraft(string key, CancellationToken cancellationToken)
    {
        return Ok(await _formBuilderService.GetDraftAsync(key, cancellationToken));
    }

    [HttpPut("{key}/draft")]
    [ProducesResponseType(typeof(FormSchemaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveDraft(string key, [FromBody] FormSchemaDto schema, CancellationToken cancellationToken)
    {
        return Ok(await _formBuilderService.SaveDraftAsync(key, schema, cancellationToken));
    }

    [HttpPost("{key}/publish")]
    [ProducesResponseType(typeof(FormSchemaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Publish(string key, CancellationToken cancellationToken)
    {
        return Ok(await _formBuilderService.PublishDraftAsync(key, cancellationToken));
    }
}
