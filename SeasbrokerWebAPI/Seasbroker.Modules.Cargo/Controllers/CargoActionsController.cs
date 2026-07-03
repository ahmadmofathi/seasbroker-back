using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.Constants;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Services;

namespace Seasbroker.Modules.Cargo.Controllers;

/// <summary>
/// Cargo lifecycle and quote promotion actions.
/// </summary>
[ApiController]
[Authorize(Policy = CargoConstants.SuperuserPolicy)]
[Tags("Cargo")]
[Route("api/cargo")]
public class CargoActionsController : ControllerBase
{
    private readonly ICargoListingService _cargoListingService;
    private readonly IQuotePromotionService _quotePromotionService;

    public CargoActionsController(
        ICargoListingService cargoListingService,
        IQuotePromotionService quotePromotionService)
    {
        _cargoListingService = cargoListingService;
        _quotePromotionService = quotePromotionService;
    }

    /// <summary>
    /// Promotes a requested quote into a cargo listing.
    /// </summary>
    [HttpPost("promote-from-quote")]
    [ProducesResponseType(typeof(CargoListingRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PromoteFromQuote(
        [FromBody] PromoteQuoteToCargoRequest request,
        CancellationToken cancellationToken)
    {
        var listing = await _quotePromotionService.PromoteAsync(
            new PromoteQuoteToCargoCommand(
                request.RequestedQuoteId,
                request.ReferenceNumber,
                request.Status,
                request.Priority),
            cancellationToken);

        return Ok(listing);
    }

    /// <summary>
    /// Closes a cargo listing, preventing further matching.
    /// </summary>
    [HttpPost("{id}/close")]
    [ProducesResponseType(typeof(CargoListingRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(string id, CancellationToken cancellationToken)
    {
        var listing = await _cargoListingService.CloseAsync(new CloseCargoListingCommand(id), cancellationToken);
        return Ok(listing);
    }

    /// <summary>
    /// Cancels a cargo listing, preventing further matching.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(CargoListingRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(string id, CancellationToken cancellationToken)
    {
        var listing = await _cargoListingService.CancelAsync(new CancelCargoListingCommand(id), cancellationToken);
        return Ok(listing);
    }
}
