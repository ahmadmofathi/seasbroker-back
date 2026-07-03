using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Approval.Application.Constants;
using Seasbroker.Modules.Approval.Application.DTOs;
using Seasbroker.Modules.Approval.Application.Queries;
using Seasbroker.Modules.Approval.Application.Services;

namespace Seasbroker.Modules.Approval.Controllers;

/// <summary>
/// Match approval workflow actions and queues.
/// </summary>
[ApiController]
[Authorize(Policy = ApprovalConstants.SuperuserPolicy)]
[Tags("Approval")]
[Route("api/matches")]
public class MatchApprovalController : ControllerBase
{
    private readonly IMatchApprovalWorkflowService _workflowService;

    public MatchApprovalController(IMatchApprovalWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>
    /// Lists matches awaiting superuser approval.
    /// </summary>
    [HttpGet("pending-approval")]
    [ProducesResponseType(typeof(PocketBaseListResponse<MatchApprovalRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PendingApproval(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _workflowService.GetPendingApprovalAsync(
            new GetPendingApprovalMatchesQuery(page, perPage),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lists approved matches.
    /// </summary>
    [HttpGet("approved")]
    [ProducesResponseType(typeof(PocketBaseListResponse<MatchApprovalRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approved(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _workflowService.GetApprovedAsync(
            new GetApprovedMatchesQuery(page, perPage),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Approves a pending match, reserves vessel capacity, and rejects competing proposals.
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(MatchApprovalRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(
        string id,
        [FromBody] MatchApprovalActionRequest? request,
        CancellationToken cancellationToken)
    {
        var match = await _workflowService.ApproveAsync(
            id,
            request?.Reason,
            request?.RowVersion,
            cancellationToken);

        return Ok(match);
    }

    /// <summary>
    /// Rejects a pending match while leaving cargo open for future matching.
    /// </summary>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(MatchApprovalRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(
        string id,
        [FromBody] MatchApprovalActionRequest? request,
        CancellationToken cancellationToken)
    {
        var match = await _workflowService.RejectAsync(
            id,
            request?.Reason,
            request?.RowVersion,
            cancellationToken);

        return Ok(match);
    }

    /// <summary>
    /// Cancels a pre-approval or approved match depending on current status.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(MatchApprovalRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        string id,
        [FromBody] MatchApprovalActionRequest? request,
        CancellationToken cancellationToken)
    {
        var match = await _workflowService.CancelAsync(
            id,
            request?.Reason,
            request?.RowVersion,
            cancellationToken);

        return Ok(match);
    }

    /// <summary>
    /// Completes an approved match and locks it from further updates.
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(MatchApprovalRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(
        string id,
        [FromBody] MatchApprovalActionRequest? request,
        CancellationToken cancellationToken)
    {
        var match = await _workflowService.CompleteAsync(
            id,
            request?.Reason,
            request?.RowVersion,
            cancellationToken);

        return Ok(match);
    }
}
