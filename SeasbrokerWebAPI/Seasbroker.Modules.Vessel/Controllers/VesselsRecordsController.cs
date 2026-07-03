using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Constants;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Queries;
using Seasbroker.Modules.Vessel.Application.Services;

namespace Seasbroker.Modules.Vessel.Controllers;

/// <summary>
/// PocketBase-compatible vessel records API for superuser administration.
/// </summary>
[ApiController]
[Authorize(Policy = VesselConstants.SuperuserPolicy)]
[Tags("Vessels")]
[Route("api/collections/vessels/records")]
public class VesselsRecordsController : ControllerBase
{
    private readonly IVesselService _vesselService;

    public VesselsRecordsController(IVesselService vesselService)
    {
        _vesselService = vesselService;
    }

    /// <summary>
    /// Lists vessels with optional status filter and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PocketBaseListResponse<VesselRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(
        [FromQuery] string? filter,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var status = TryParseStatusFilter(filter);
        var result = await _vesselService.GetAllAsync(
            new GetVesselsQuery(status, page, perPage),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets a single vessel by identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VesselRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> View(string id, CancellationToken cancellationToken)
    {
        var vessel = await _vesselService.GetByIdAsync(new GetVesselByIdQuery(id), cancellationToken);
        return Ok(vessel);
    }

    /// <summary>
    /// Creates a new vessel.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VesselRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateVesselRequest request,
        CancellationToken cancellationToken)
    {
        var vessel = await _vesselService.CreateAsync(
            new CreateVesselCommand(
                request.Name,
                request.ImoNumber,
                request.VesselType,
                request.Dwt,
                request.TeuCapacity,
                request.LengthOverall,
                request.Beam,
                request.Draft,
                request.CurrentPort,
                request.FlagCountry,
                request.Status,
                request.Customer,
                request.Notes),
            cancellationToken);

        return Ok(vessel);
    }

    /// <summary>
    /// Updates an existing vessel. IMO number cannot be changed.
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(VesselRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateVesselRequest request,
        CancellationToken cancellationToken)
    {
        var vessel = await _vesselService.UpdateAsync(
            new UpdateVesselCommand(
                id,
                request.Name,
                request.VesselType,
                request.Dwt,
                request.TeuCapacity,
                request.LengthOverall,
                request.Beam,
                request.Draft,
                request.CurrentPort,
                request.FlagCountry,
                request.Status,
                request.Customer,
                request.Notes),
            cancellationToken);

        return Ok(vessel);
    }

    /// <summary>
    /// Soft-deletes a vessel by setting status to Inactive and deactivating availability windows.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _vesselService.DeactivateAsync(new DeactivateVesselCommand(id), cancellationToken);
        return NoContent();
    }

    private static string? TryParseStatusFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        const string prefix = "status = ";
        var trimmed = filter.Trim();

        if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        return trimmed[prefix.Length..].Trim().Trim('"');
    }
}
