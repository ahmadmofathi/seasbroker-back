using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Services;

namespace Seasbroker.Modules.Forms.Controllers;

/// <summary>
/// Lets a customer change the answers on their own request until the team accepts it. Like tracking, a
/// request is identified by its tracking number plus the email it was registered with, sent in the body.
/// </summary>
[ApiController]
[Tags("Tracking")]
[Route("api/track")]
public class RequestEditController : ControllerBase
{
    private readonly IFormSubmissionService _submissions;

    public RequestEditController(IFormSubmissionService submissions)
    {
        _submissions = submissions;
    }

    /// <summary>Loads the customer's request in the form it was registered with. 409 once it can no longer be edited.</summary>
    [HttpPost("edit-form")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RequestEditFormDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LoadForEdit([FromBody] RequestEditLookupDto request, CancellationToken cancellationToken) =>
        Ok(await _submissions.LoadForEditAsync(request.Number, request.Email, cancellationToken));

    /// <summary>Saves the customer's changes. The answers are validated exactly as when registering.</summary>
    [HttpPut("edit")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Save([FromBody] RequestEditSaveDto request, CancellationToken cancellationToken)
    {
        await _submissions.UpdateAsync(request.Number, request.Email, request.Values, cancellationToken);
        return NoContent();
    }
}
