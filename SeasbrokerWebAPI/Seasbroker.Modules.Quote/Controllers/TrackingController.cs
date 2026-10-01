using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Quote.Application.DTOs;
using Seasbroker.Modules.Quote.Application.Services;

namespace Seasbroker.Modules.Quote.Controllers;

/// <summary>
/// Public request tracking for customers.
/// </summary>
[ApiController]
[Tags("Tracking")]
[Route("api/track")]
public class TrackingController : ControllerBase
{
    private readonly IRequestTrackingService _trackingService;

    public TrackingController(IRequestTrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    /// <summary>
    /// Looks up a request by its tracking number (or cargo listing reference) and the customer's email.
    /// A POST so the email travels in the body rather than in a URL that ends up in access logs.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RequestTrackingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Track(
        [FromBody] TrackRequestDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await _trackingService.TrackAsync(request.Number, request.Email, cancellationToken));
    }
}
