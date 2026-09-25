using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Constants;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Exceptions;
using Seasbroker.Modules.Vessel.Application.Helpers;
using Seasbroker.Modules.Vessel.Application.Mapping;

namespace Seasbroker.Modules.Vessel.Application.Handlers.Commands;

/// <summary>
/// Adds the vessel from a Ship Brokerage request to the fleet, with an availability window built
/// from the request's open port/date or scheduled route, so matching can use it straight away.
/// </summary>
public class PromoteQuoteToVesselCommandHandler
    : ICommandHandler<PromoteQuoteToVesselCommand, PromoteQuoteToVesselResultDto>
{
    private readonly SeasbrokerDbContext _dbContext;

    public PromoteQuoteToVesselCommandHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PromoteQuoteToVesselResultDto> HandleAsync(
        PromoteQuoteToVesselCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(command.RequestedQuoteId, out var quoteId))
        {
            throw new VesselException("The requested quote wasn't found.", StatusCodes.Status404NotFound);
        }

        var quote = await _dbContext.RequestedQuotes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken)
            ?? throw new VesselException("The requested quote wasn't found.", StatusCodes.Status404NotFound);

        var existing = await _dbContext.Vessels
            .AsNoTracking()
            .Where(v => v.RequestedQuoteId == quoteId)
            .Select(v => v.Name)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
        {
            throw new VesselException(
                $"This request is already in the fleet as '{existing}'.",
                StatusCodes.Status409Conflict);
        }

        var submission = await _dbContext.FormSubmissions
            .AsNoTracking()
            .Where(s => s.RequestedQuoteId == quoteId)
            .Select(s => new
            {
                FormKey = s.FormVersion.FormDefinition.Key,
                Values = s.Values.Select(v => new { v.FieldKey, v.ValueText }).ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (submission?.FormKey != FormDefinition.ShipRequestKey)
        {
            throw new VesselException(
                "Only Ship Brokerage requests can be added to the fleet.",
                StatusCodes.Status400BadRequest);
        }

        var values = submission.Values
            .GroupBy(v => v.FieldKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().ValueText, StringComparer.OrdinalIgnoreCase);

        var mapped = ShipRequestMapper.MapVessel(values, quote.CargoType, quote.Weight, quote.DeparturePort);
        EnsureUsable(mapped);

        if (!string.IsNullOrWhiteSpace(mapped.ImoNumber))
        {
            var sameImo = await _dbContext.Vessels
                .AsNoTracking()
                .Where(v => v.ImoNumber == mapped.ImoNumber)
                .Select(v => v.Name)
                .FirstOrDefaultAsync(cancellationToken);
            if (sameImo is not null)
            {
                throw new VesselException(
                    $"A vessel with IMO {mapped.ImoNumber} is already in the fleet ('{sameImo}').",
                    StatusCodes.Status409Conflict);
            }
        }

        var vessel = new global::Seasbroker.Infrastructure.Persistence.Entities.Vessel
        {
            Name = mapped.Name,
            ImoNumber = mapped.ImoNumber,
            VesselType = mapped.VesselType!,
            Dwt = mapped.Dwt,
            TeuCapacity = mapped.TeuCapacity,
            LengthOverall = mapped.LengthOverall,
            Beam = mapped.Beam,
            Draft = mapped.Draft,
            CurrentPort = mapped.CurrentPort,
            FlagCountry = mapped.FlagCountry,
            Status = VesselStatus.Active,
            CustomerId = quote.CustomerId,
            RequestedQuoteId = quote.Id,
            Notes = Truncate(quote.AdditionalInfo, 2000),
        };
        _dbContext.Vessels.Add(vessel);

        var result = new PromoteQuoteToVesselResultDto();
        var availability = BuildAvailability(vessel, values, result);
        if (availability is not null)
        {
            _dbContext.VesselAvailabilities.Add(availability);
        }

        // Before ship requests could only be promoted to cargo, some were turned into cargo listings
        // by mistake. Cancel that listing (kept, not deleted) and detach it from the request.
        var wrongListing = await _dbContext.CargoListings
            .FirstOrDefaultAsync(c => c.RequestedQuoteId == quoteId, cancellationToken);
        if (wrongListing is not null)
        {
            wrongListing.Status = CargoStatus.Cancelled;
            wrongListing.RequestedQuoteId = null;
            wrongListing.AdditionalInfo = Truncate(
                $"[Cancelled: created by mistake from a Ship Brokerage request; added to the fleet as vessel '{vessel.Name}' instead.] {wrongListing.AdditionalInfo}",
                2000);
            result.CancelledCargoListingReference = wrongListing.ReferenceNumber;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        result.Vessel = VesselMapper.ToRecordDto(vessel);
        result.Availability = availability is null ? null : VesselMapper.ToRecordDto(availability);
        return result;
    }

    private static void EnsureUsable(ShipRequestVessel mapped)
    {
        string? problem = null;
        if (mapped.Name.Length < 2)
        {
            problem = "it has no vessel name";
        }
        else if (mapped.VesselType is null)
        {
            problem = "its vessel type isn't one the fleet supports";
        }
        else if (mapped.Dwt <= 0)
        {
            problem = "it has no DWT";
        }
        else if (mapped.CurrentPort.Length < 2)
        {
            problem = "it has no current / open port";
        }

        if (problem is not null)
        {
            throw new VesselException(
                $"This request can't be added to the fleet: {problem}. Add the vessel manually from the Vessels page.",
                StatusCodes.Status400BadRequest);
        }
    }

    private static VesselAvailability? BuildAvailability(
        global::Seasbroker.Infrastructure.Persistence.Entities.Vessel vessel,
        IReadOnlyDictionary<string, string?> values,
        PromoteQuoteToVesselResultDto result)
    {
        var plan = ShipRequestMapper.MapAvailability(values, vessel.CurrentPort);
        if (plan is null)
        {
            result.AvailabilityNote = "The request has no open date or route, so no availability window was added.";
            return null;
        }

        try
        {
            VesselDomainHelper.ValidateAvailabilityDateRange(plan.AvailableFrom, plan.AvailableTo);
            var route = VesselDomainHelper.BuildRoute(plan.RouteStops, plan.AvailableFrom, plan.AvailableTo);

            return new VesselAvailability
            {
                VesselId = vessel.Id,
                AvailableFrom = plan.AvailableFrom,
                AvailableTo = plan.AvailableTo,
                OpenPort = route[0].Port,
                DestinationPort = route.Count > 1 ? route[^1].Port : plan.DestinationPort,
                RouteStops = route,
                IsActive = true,
            };
        }
        catch (VesselException ex)
        {
            // The vessel is still worth adding; the admin can fix the window from the Availability dialog.
            result.AvailabilityNote = $"No availability window was added: {ex.Message}";
            return null;
        }
    }

    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];
}
