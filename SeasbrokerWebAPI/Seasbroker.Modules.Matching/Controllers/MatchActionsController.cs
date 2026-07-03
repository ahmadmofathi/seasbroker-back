using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Matching.Application.Commands;
using Seasbroker.Modules.Matching.Application.Constants;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Services;

namespace Seasbroker.Modules.Matching.Controllers;

/// <summary>
/// Match lifecycle actions (expire).
/// </summary>
[ApiController]
[Authorize(Policy = MatchingConstants.SuperuserPolicy)]
[Tags("Matching")]
[Route("api/matches")]
public class MatchActionsController : ControllerBase
{
    private readonly IMatchLifecycleService _matchLifecycleService;

    public MatchActionsController(IMatchLifecycleService matchLifecycleService)
    {
        _matchLifecycleService = matchLifecycleService;
    }

    /// <summary>
    /// Expires a proposed or pending approval match.
    /// </summary>
    [HttpPost("{id}/expire")]
    [ProducesResponseType(typeof(MatchRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Expire(string id, CancellationToken cancellationToken)
    {
        var match = await _matchLifecycleService.ExpireAsync(new ExpireMatchCommand(id), cancellationToken);
        return Ok(match);
    }
}
