using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Quote.Application.Commands;
using Seasbroker.Modules.Quote.Application.DTOs;
using Seasbroker.Modules.Quote.Application.Services;

namespace Seasbroker.Modules.Quote.Controllers;

[ApiController]
[Route("api/quote")]
public class QuoteController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuoteController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CreateQuoteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuoteRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new PocketBaseErrorResponse
            {
                Message = "Missing request fields",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var response = await _quoteService.CreateAsync(
            new CreateQuoteCommand(
                request.CargoType,
                request.Weight,
                request.DeparturePort,
                request.DepartureTime,
                request.ArrivalPort,
                request.ArrivalTime,
                request.Dimensions,
                request.AdditionalInfo,
                request.Fname,
                request.Lname,
                request.Email,
                request.PhoneNumber),
            cancellationToken);

        return Ok(response);
    }
}
