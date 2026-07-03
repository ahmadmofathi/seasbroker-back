using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Cargo.Application.Abstractions;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Exceptions;
using Seasbroker.Modules.Cargo.Application.Helpers;
using Seasbroker.Modules.Cargo.Application.Mapping;
using Seasbroker.Modules.Cargo.Application.Queries;

namespace Seasbroker.Modules.Cargo.Application.Handlers.Queries;

public class GetCargoListingsQueryHandler
    : IQueryHandler<GetCargoListingsQuery, PocketBaseListResponse<CargoListingRecordDto>>
{
    private readonly SeasbrokerDbContext _dbContext;

    public GetCargoListingsQueryHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PocketBaseListResponse<CargoListingRecordDto>> HandleAsync(
        GetCargoListingsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var perPage = query.PerPage < 1 ? 50 : Math.Min(query.PerPage, 200);

        var listingsQuery = _dbContext.CargoListings.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            listingsQuery = listingsQuery.Where(c => c.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            if (Guid.TryParse(query.CustomerId, out var customerId))
            {
                listingsQuery = listingsQuery.Where(c => c.CustomerId == customerId);
            }
        }

        var totalItems = await listingsQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)perPage);

        var listings = await listingsQuery
            .OrderByDescending(c => c.Created)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        return new PocketBaseListResponse<CargoListingRecordDto>
        {
            Page = page,
            PerPage = perPage,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = listings.Select(CargoMapper.ToRecordDto).ToList(),
        };
    }
}

public class GetCargoListingByIdQueryHandler : IQueryHandler<GetCargoListingByIdQuery, CargoListingRecordDto>
{
    private readonly SeasbrokerDbContext _dbContext;

    public GetCargoListingByIdQueryHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CargoListingRecordDto> HandleAsync(
        GetCargoListingByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var cargoListingId = CargoDomainHelper.ParseCargoListingId(query.CargoListingId);

        var listing = await _dbContext.CargoListings
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cargoListingId, cancellationToken);

        if (listing is null)
        {
            throw new CargoException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return CargoMapper.ToRecordDto(listing);
    }
}

public class GetOpenCargoForMatchingQueryHandler
    : IQueryHandler<GetOpenCargoForMatchingQuery, IReadOnlyList<CargoListingRecordDto>>
{
    private readonly SeasbrokerDbContext _dbContext;

    public GetOpenCargoForMatchingQueryHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CargoListingRecordDto>> HandleAsync(
        GetOpenCargoForMatchingQuery query,
        CancellationToken cancellationToken = default)
    {
        var listingsQuery = _dbContext.CargoListings
            .AsNoTracking()
            .Where(c => c.Status == CargoStatus.Open);

        if (!string.IsNullOrWhiteSpace(query.DeparturePort))
        {
            listingsQuery = listingsQuery.Where(c => c.DeparturePort == query.DeparturePort.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.ArrivalPort))
        {
            listingsQuery = listingsQuery.Where(c => c.ArrivalPort == query.ArrivalPort.Trim());
        }

        if (query.DepartureFrom.HasValue)
        {
            listingsQuery = listingsQuery.Where(c => c.DepartureTime >= query.DepartureFrom.Value);
        }

        if (query.ArrivalTo.HasValue)
        {
            listingsQuery = listingsQuery.Where(c => c.ArrivalTime <= query.ArrivalTo.Value);
        }

        var listings = await listingsQuery
            .OrderByDescending(c => c.Priority)
            .ThenByDescending(c => c.Created)
            .ToListAsync(cancellationToken);

        return listings.Select(CargoMapper.ToRecordDto).ToList();
    }
}

public class GetCargoByQuoteIdQueryHandler
    : IQueryHandler<GetCargoByQuoteIdQuery, CargoListingRecordDto?>
{
    private readonly SeasbrokerDbContext _dbContext;

    public GetCargoByQuoteIdQueryHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CargoListingRecordDto?> HandleAsync(
        GetCargoByQuoteIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var quoteId = CargoDomainHelper.ParseOptionalQuoteId(query.RequestedQuoteId);

        if (!quoteId.HasValue)
        {
            return null;
        }

        var listing = await _dbContext.CargoListings
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.RequestedQuoteId == quoteId.Value, cancellationToken);

        return listing is null ? null : CargoMapper.ToRecordDto(listing);
    }
}
