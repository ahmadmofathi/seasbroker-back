using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Constants;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Helpers;
using Seasbroker.Modules.Vessel.Application.Queries;
using Seasbroker.Modules.Vessel.Application.Services;

namespace Seasbroker.Modules.Vessel.Controllers;

/// <summary>
/// PocketBase-compatible vessel availability records API.
/// </summary>
[ApiController]
[Authorize(Policy = VesselConstants.SuperuserPolicy)]
[Tags("Vessel Availabilities")]
[Route("api/collections/vesselAvailabilities/records")]
public class VesselAvailabilitiesRecordsController : ControllerBase
{
    private readonly IVesselAvailabilityService _availabilityService;

    public VesselAvailabilitiesRecordsController(IVesselAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    /// <summary>
    /// Lists availability windows. Supports PocketBase filter: vesselId = "{id}".
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PocketBaseListResponse<VesselAvailabilityRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(
        [FromQuery] string? filter,
        CancellationToken cancellationToken)
    {
        var vesselId = PocketBaseFilterParser.TryParseVesselIdEquals(filter) ?? string.Empty;
        var items = await _availabilityService.GetByVesselIdAsync(
            new GetVesselAvailabilitiesQuery(vesselId),
            cancellationToken);

        return Ok(new PocketBaseListResponse<VesselAvailabilityRecordDto>
        {
            Page = 1,
            PerPage = items.Count,
            TotalItems = items.Count,
            TotalPages = 1,
            Items = items,
        });
    }

    /// <summary>
    /// Creates an active availability window for a vessel.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VesselAvailabilityRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateVesselAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var availability = await _availabilityService.CreateAsync(
            new CreateVesselAvailabilityCommand(
                request.VesselId,
                request.AvailableFrom,
                request.AvailableTo,
                request.OpenPort,
                request.DestinationPort),
            cancellationToken);

        return Ok(availability);
    }

    /// <summary>
    /// Updates an availability window.
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(VesselAvailabilityRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateVesselAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var availability = await _availabilityService.UpdateAsync(
            new UpdateVesselAvailabilityCommand(
                id,
                request.AvailableFrom,
                request.AvailableTo,
                request.OpenPort,
                request.DestinationPort,
                request.IsActive),
            cancellationToken);

        return Ok(availability);
    }

    /// <summary>
    /// Deactivates an availability window (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _availabilityService.DeactivateAsync(
            new DeactivateVesselAvailabilityCommand(id),
            cancellationToken);

        return NoContent();
    }
}
