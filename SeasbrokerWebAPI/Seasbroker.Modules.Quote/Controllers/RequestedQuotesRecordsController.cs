using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Quote.Application.Constants;
using Seasbroker.Modules.Quote.Application.DTOs;
using Seasbroker.Modules.Quote.Application.Queries;
using Seasbroker.Modules.Quote.Application.Services;

namespace Seasbroker.Modules.Quote.Controllers;

/// <summary>
/// PocketBase-compatible requested quote records API for superuser administration.
/// </summary>
[ApiController]
[Authorize(Policy = QuoteConstants.SuperuserPolicy)]
[Tags("Quotes")]
[Route("api/collections/requestedQuotes/records")]
public class RequestedQuotesRecordsController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public RequestedQuotesRecordsController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    /// <summary>
    /// Lists public quote requests submitted from website forms.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PocketBaseListResponse<RequestedQuoteRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _quoteService.GetAllAsync(
            new GetRequestedQuotesQuery(page, perPage),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets a single requested quote by identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RequestedQuoteRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> View(string id, CancellationToken cancellationToken)
    {
        var quote = await _quoteService.GetByIdAsync(id, cancellationToken);
        if (quote is null)
        {
            return NotFound(new PocketBaseErrorResponse
            {
                Message = "Requested quote not found.",
                Status = StatusCodes.Status404NotFound,
            });
        }

        return Ok(quote);
    }
}
