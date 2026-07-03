using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Matching.Application.Commands;
using Seasbroker.Modules.Matching.Application.Constants;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Queries;
using Seasbroker.Modules.Matching.Application.Services;

namespace Seasbroker.Modules.Matching.Controllers;

/// <summary>
/// PocketBase-compatible matching rule records API for superuser administration.
/// </summary>
[ApiController]
[Authorize(Policy = MatchingConstants.SuperuserPolicy)]
[Tags("Matching")]
[Route("api/collections/matchingRules/records")]
public class MatchingRulesRecordsController : ControllerBase
{
    private readonly IMatchingRuleRecordsService _matchingRuleRecordsService;

    public MatchingRulesRecordsController(IMatchingRuleRecordsService matchingRuleRecordsService)
    {
        _matchingRuleRecordsService = matchingRuleRecordsService;
    }

    /// <summary>
    /// Lists all matching rules ordered by criterion.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MatchingRuleRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var rules = await _matchingRuleRecordsService.GetAllAsync(new GetMatchingRulesQuery(), cancellationToken);
        return Ok(rules);
    }

    /// <summary>
    /// Updates a matching rule weight, active flag, or configuration.
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(MatchingRuleRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateMatchingRuleRequest request,
        CancellationToken cancellationToken)
    {
        var rule = await _matchingRuleRecordsService.UpdateAsync(
            new UpdateMatchingRuleCommand(id, request.Weight, request.IsActive, request.Configuration),
            cancellationToken);

        return Ok(rule);
    }
}
