using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Constants;
using Seasbroker.Modules.Vessel.Application.DTOs;

namespace Seasbroker.Modules.Vessel.Controllers;

/// <summary>
/// Vessel actions that aren't plain record CRUD.
/// </summary>
[ApiController]
[Authorize(Policy = VesselConstants.SuperuserPolicy)]
[Tags("Vessels")]
[Route("api/vessels")]
public class VesselActionsController : ControllerBase
{
    private readonly ICommandHandler<PromoteQuoteToVesselCommand, PromoteQuoteToVesselResultDto> _promoteHandler;

    public VesselActionsController(ICommandHandler<PromoteQuoteToVesselCommand, PromoteQuoteToVesselResultDto> promoteHandler)
    {
        _promoteHandler = promoteHandler;
    }

    /// <summary>
    /// Adds the vessel from a Ship Brokerage request to the fleet, with an availability window
    /// built from the request's open port/date or scheduled route.
    /// </summary>
    [HttpPost("promote-from-quote")]
    [ProducesResponseType(typeof(PromoteQuoteToVesselResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PromoteFromQuote(
        [FromBody] PromoteQuoteToVesselRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _promoteHandler.HandleAsync(
            new PromoteQuoteToVesselCommand(request.RequestedQuoteId),
            cancellationToken);

        return Ok(result);
    }
}
