using FluentValidation;
using Microsoft.AspNetCore.Http;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Exceptions;
using Seasbroker.Modules.Vessel.Application.Helpers;
using Seasbroker.Modules.Vessel.Application.Mapping;
using Seasbroker.Modules.Vessel.Application.Validators;

namespace Seasbroker.Modules.Vessel.Application.Handlers.Commands;

public class UpdateVesselAvailabilityCommandHandler
    : ICommandHandler<UpdateVesselAvailabilityCommand, VesselAvailabilityRecordDto>
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IValidator<UpdateVesselAvailabilityCommand> _validator;

    public UpdateVesselAvailabilityCommandHandler(
        SeasbrokerDbContext dbContext,
        IValidator<UpdateVesselAvailabilityCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<VesselAvailabilityRecordDto> HandleAsync(
        UpdateVesselAvailabilityCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);

        if (!Guid.TryParse(command.AvailabilityId, out var availabilityId))
        {
            throw new VesselException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        var availability = await VesselDomainHelper.GetAvailabilityOrThrowAsync(
            _dbContext,
            availabilityId,
            cancellationToken);

        var from = command.AvailableFrom ?? availability.AvailableFrom;
        var to = command.AvailableTo ?? availability.AvailableTo;
        var willBeActive = command.IsActive ?? availability.IsActive;

        if (command.AvailableFrom.HasValue || command.AvailableTo.HasValue)
        {
            VesselDomainHelper.ValidateAvailabilityDateRange(from, to);
        }

        // A new route replaces the old one; otherwise the existing route is re-checked so that
        // moving the window can't leave port ETAs outside it.
        var route = command.RouteStops is not null
            ? VesselDomainHelper.BuildRoute(command.RouteStops, from, to)
            : VesselDomainHelper.BuildRoute(
                (availability.RouteStops ?? new()).Select(s => new RouteStopDto { Port = s.Port, Eta = s.Eta }).ToList(),
                from,
                to);

        if (willBeActive)
        {
            var activeWindows = await VesselDomainHelper.GetActiveAvailabilityWindowsAsync(
                _dbContext,
                availability.VesselId,
                cancellationToken);

            try
            {
                VesselAvailabilityOverlapValidator.EnsureNoOverlap(
                    from,
                    to,
                    activeWindows,
                    availability.Id);
            }
            catch (InvalidOperationException ex)
            {
                throw new VesselException(ex.Message, StatusCodes.Status409Conflict);
            }
        }

        if (command.AvailableFrom.HasValue)
        {
            availability.AvailableFrom = command.AvailableFrom.Value;
        }

        if (command.AvailableTo.HasValue)
        {
            availability.AvailableTo = command.AvailableTo.Value;
        }

        if (command.OpenPort is not null)
        {
            availability.OpenPort = command.OpenPort.Trim();
        }

        if (command.DestinationPort is not null)
        {
            availability.DestinationPort = command.DestinationPort.Trim();
        }

        if (command.IsActive.HasValue)
        {
            availability.IsActive = command.IsActive.Value;
        }

        if (command.RouteStops is not null)
        {
            availability.RouteStops = route;
            if (route.Count > 0)
            {
                availability.OpenPort = route[0].Port;
                availability.DestinationPort = route.Count > 1 ? route[^1].Port : availability.DestinationPort;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return VesselMapper.ToRecordDto(availability);
    }
}
