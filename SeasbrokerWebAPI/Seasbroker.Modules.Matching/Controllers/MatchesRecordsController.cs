using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Matching.Application.Constants;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Helpers;
using Seasbroker.Modules.Matching.Application.Queries;
using Seasbroker.Modules.Matching.Application.Services;

namespace Seasbroker.Modules.Matching.Controllers;

/// <summary>
/// PocketBase-compatible match records API for superuser administration.
/// </summary>
[ApiController]
[Authorize(Policy = MatchingConstants.SuperuserPolicy)]
[Tags("Matching")]
[Route("api/collections/matches/records")]
public class MatchesRecordsController : ControllerBase
{
    private readonly IMatchRecordsService _matchRecordsService;

    public MatchesRecordsController(IMatchRecordsService matchRecordsService)
    {
        _matchRecordsService = matchRecordsService;
    }

    /// <summary>
    /// Lists matches with optional status, cargo, or vessel filter and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PocketBaseListResponse<MatchRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(
        [FromQuery] string? filter,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var status = PocketBaseFilterParser.TryParseEquals(filter, "status");
        var cargoListingId = PocketBaseFilterParser.TryParseEquals(filter, "cargoListingId");
        var vesselId = PocketBaseFilterParser.TryParseEquals(filter, "vesselId");

        var result = await _matchRecordsService.GetAllAsync(
            new GetMatchesQuery(status, cargoListingId, vesselId, page, perPage),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets a single match by identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MatchRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> View(string id, CancellationToken cancellationToken)
    {
        var match = await _matchRecordsService.GetByIdAsync(new GetMatchByIdQuery(id), cancellationToken);
        return Ok(match);
    }
}
