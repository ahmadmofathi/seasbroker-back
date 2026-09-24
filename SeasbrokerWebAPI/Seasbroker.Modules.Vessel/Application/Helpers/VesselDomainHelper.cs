using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Constants;
using Seasbroker.Modules.Vessel.Application.DTOs;
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

    /// <summary>
    /// Checks an ordered route and turns it into stored stops: every stop needs a port, ETAs must go
    /// strictly forward, the same port can't repeat back-to-back, and every ETA must fall inside the
    /// availability window.
    /// </summary>
    public static List<RouteStop> BuildRoute(IReadOnlyList<RouteStopDto> stops, DateTime from, DateTime to)
    {
        if (stops.Count > VesselConstants.MaxRouteStops)
        {
            throw new VesselException(
                $"A route can have at most {VesselConstants.MaxRouteStops} ports.",
                StatusCodes.Status400BadRequest);
        }

        var route = new List<RouteStop>();
        for (var i = 0; i < stops.Count; i++)
        {
            var port = stops[i].Port?.Trim() ?? string.Empty;
            var eta = stops[i].Eta;
            var label = i == 0 ? "Next port" : $"Port {i + 1}";

            if (port.Length is < 2 or > 200)
            {
                throw new VesselException($"{label} needs a port.", StatusCodes.Status400BadRequest);
            }

            if (eta < from || eta > to)
            {
                throw new VesselException(
                    $"{label} ETA must fall within the availability window.",
                    StatusCodes.Status400BadRequest);
            }

            if (i > 0)
            {
                var previous = route[i - 1];
                if (string.Equals(previous.Port, port, StringComparison.OrdinalIgnoreCase))
                {
                    throw new VesselException($"{label} repeats the port before it.", StatusCodes.Status400BadRequest);
                }

                if (eta <= previous.Eta)
                {
                    throw new VesselException(
                        $"{label} ETA must be after the previous port's ETA.",
                        StatusCodes.Status400BadRequest);
                }
            }

            route.Add(new RouteStop { Port = port, Eta = eta });
        }

        return route;
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
