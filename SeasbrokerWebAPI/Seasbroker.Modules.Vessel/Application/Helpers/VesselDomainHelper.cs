using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Constants;
using Seasbroker.Modules.Vessel.Application.Exceptions;

namespace Seasbroker.Modules.Vessel.Application.Helpers;

internal static class VesselDomainHelper
{
    public static async Task EnsureCustomerExistsAsync(
        SeasbrokerDbContext dbContext,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Id == customerId, cancellationToken);

        if (!exists)
        {
            throw new VesselException("The requested customer wasn't found.", StatusCodes.Status404NotFound);
        }
    }

    public static async Task EnsureImoUniqueAsync(
        SeasbrokerDbContext dbContext,
        string? imoNumber,
        Guid? excludeVesselId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(imoNumber))
        {
            return;
        }

        var query = dbContext.Vessels.AsNoTracking().Where(v => v.ImoNumber == imoNumber);

        if (excludeVesselId.HasValue)
        {
            query = query.Where(v => v.Id != excludeVesselId.Value);
        }

        if (await query.AnyAsync(cancellationToken))
        {
            throw new VesselException(
                "A vessel with this IMO number already exists.",
                StatusCodes.Status409Conflict);
        }
    }

    public static Guid ParseVesselId(string vesselId)
    {
        if (!Guid.TryParse(vesselId, out var parsedId))
        {
            throw new VesselException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return parsedId;
    }

    public static Guid? ParseOptionalCustomerId(string? customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return null;
        }

        if (!Guid.TryParse(customerId, out var parsedId))
        {
            throw new VesselException("Invalid customer identifier.", StatusCodes.Status400BadRequest);
        }

        return parsedId;
    }

    public static void ValidateAvailabilityDateRange(DateTime from, DateTime to)
    {
        if (from >= to)
        {
            throw new VesselException("AvailableFrom must be before AvailableTo.", StatusCodes.Status400BadRequest);
        }

        if ((to - from).TotalDays > VesselConstants.MaxAvailabilityWindowDays)
        {
            throw new VesselException(
                $"Availability window cannot exceed {VesselConstants.MaxAvailabilityWindowDays} days.",
                StatusCodes.Status400BadRequest);
        }
    }

    public static async Task<List<(Guid Id, DateTime From, DateTime To)>> GetActiveAvailabilityWindowsAsync(
        SeasbrokerDbContext dbContext,
        Guid vesselId,
        CancellationToken cancellationToken)
    {
        return await dbContext.VesselAvailabilities
            .AsNoTracking()
            .Where(a => a.VesselId == vesselId && a.IsActive)
            .Select(a => new ValueTuple<Guid, DateTime, DateTime>(a.Id, a.AvailableFrom, a.AvailableTo))
            .ToListAsync(cancellationToken);
    }

    public static async Task<global::Seasbroker.Infrastructure.Persistence.Entities.Vessel> GetVesselOrThrowAsync(
        SeasbrokerDbContext dbContext,
        Guid vesselId,
        CancellationToken cancellationToken)
    {
        var vessel = await dbContext.Vessels
            .FirstOrDefaultAsync(v => v.Id == vesselId, cancellationToken);

        if (vessel is null)
        {
            throw new VesselException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return vessel;
    }

    public static async Task<VesselAvailability> GetAvailabilityOrThrowAsync(
        SeasbrokerDbContext dbContext,
        Guid availabilityId,
        CancellationToken cancellationToken)
    {
        var availability = await dbContext.VesselAvailabilities
            .FirstOrDefaultAsync(a => a.Id == availabilityId, cancellationToken);

        if (availability is null)
        {
            throw new VesselException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        return availability;
    }
}
