using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.Constants;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Helpers;
using Seasbroker.Modules.Cargo.Application.Queries;
using Seasbroker.Modules.Cargo.Application.Services;

namespace Seasbroker.Modules.Cargo.Controllers;

/// <summary>
/// PocketBase-compatible cargo listing records API for superuser administration.
/// </summary>
[ApiController]
[Authorize(Policy = CargoConstants.SuperuserPolicy)]
[Tags("Cargo")]
[Route("api/collections/cargoListings/records")]
public class CargoListingsRecordsController : ControllerBase
{
    private readonly ICargoListingService _cargoListingService;

    public CargoListingsRecordsController(ICargoListingService cargoListingService)
    {
        _cargoListingService = cargoListingService;
    }

    /// <summary>
    /// Lists cargo listings with optional status or customer filter and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PocketBaseListResponse<CargoListingRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(
        [FromQuery] string? filter,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var status = PocketBaseFilterParser.TryParseStatusEquals(filter);
        var customerId = PocketBaseFilterParser.TryParseCustomerEquals(filter);

        var result = await _cargoListingService.GetAllAsync(
            new GetCargoListingsQuery(status, customerId, page, perPage),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets a single cargo listing by identifier.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CargoListingRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> View(string id, CancellationToken cancellationToken)
    {
        var listing = await _cargoListingService.GetByIdAsync(new GetCargoListingByIdQuery(id), cancellationToken);
        return Ok(listing);
    }

    /// <summary>
    /// Creates a new cargo listing. Reference number is auto-generated when omitted.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CargoListingRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCargoListingRequest request,
        CancellationToken cancellationToken)
    {
        var listing = await _cargoListingService.CreateAsync(
            new CreateCargoListingCommand(
                request.Customer,
                request.RequestedQuote,
                request.ReferenceNumber,
                request.CargoType,
                request.Weight,
                request.Dimensions,
                request.DeparturePort,
                request.DepartureTime,
                request.ArrivalPort,
                request.ArrivalTime,
                request.AdditionalInfo,
                request.Status,
                request.Priority),
            cancellationToken);

        return Ok(listing);
    }

    /// <summary>
    /// Updates an open cargo listing.
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(CargoListingRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateCargoListingRequest request,
        CancellationToken cancellationToken)
    {
        var listing = await _cargoListingService.UpdateAsync(
            new UpdateCargoListingCommand(
                id,
                request.CargoType,
                request.Weight,
                request.Dimensions,
                request.DeparturePort,
                request.DepartureTime,
                request.ArrivalPort,
                request.ArrivalTime,
                request.AdditionalInfo,
                request.Priority),
            cancellationToken);

        return Ok(listing);
    }
}
