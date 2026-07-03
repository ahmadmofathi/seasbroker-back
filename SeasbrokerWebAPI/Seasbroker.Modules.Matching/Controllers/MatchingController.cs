using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Matching.Application.Commands;
using Seasbroker.Modules.Matching.Application.Constants;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Services;

namespace Seasbroker.Modules.Matching.Controllers;

/// <summary>
/// Matching engine run and manual match creation endpoints.
/// </summary>
[ApiController]
[Authorize(Policy = MatchingConstants.SuperuserPolicy)]
[Tags("Matching")]
[Route("api/matching")]
public class MatchingController : ControllerBase
{
    private readonly IMatchingRunService _matchingRunService;
    private readonly IMatchLifecycleService _matchLifecycleService;

    public MatchingController(
        IMatchingRunService matchingRunService,
        IMatchLifecycleService matchLifecycleService)
    {
        _matchingRunService = matchingRunService;
        _matchLifecycleService = matchLifecycleService;
    }

    /// <summary>
    /// Runs the matching engine for a cargo listing, vessel, or all open cargo when no scope is provided.
    /// </summary>
    [HttpPost("run")]
    [ProducesResponseType(typeof(MatchingRunResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Run(
        [FromBody] RunMatchingRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _matchingRunService.RunAsync(
            new RunMatchingCommand(request?.CargoListingId, request?.VesselId),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Creates a manual match between a cargo listing and a vessel.
    /// </summary>
    [HttpPost("manual")]
    [ProducesResponseType(typeof(MatchRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateManual(
        [FromBody] CreateManualMatchRequest request,
        CancellationToken cancellationToken)
    {
        var match = await _matchLifecycleService.CreateManualAsync(
            new CreateManualMatchCommand(
                request.CargoListingId,
                request.VesselId,
                request.Score,
                request.MatchReason),
            cancellationToken);

        return Ok(match);
    }
}
