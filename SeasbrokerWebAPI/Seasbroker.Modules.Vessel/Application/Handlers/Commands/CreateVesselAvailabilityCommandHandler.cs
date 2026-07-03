using FluentValidation;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Exceptions;
using Seasbroker.Modules.Vessel.Application.Helpers;
using Seasbroker.Modules.Vessel.Application.Mapping;
using Seasbroker.Modules.Vessel.Application.Validators;
using Microsoft.AspNetCore.Http;

namespace Seasbroker.Modules.Vessel.Application.Handlers.Commands;

public class CreateVesselAvailabilityCommandHandler
    : ICommandHandler<CreateVesselAvailabilityCommand, VesselAvailabilityRecordDto>
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IValidator<CreateVesselAvailabilityCommand> _validator;

    public CreateVesselAvailabilityCommandHandler(
        SeasbrokerDbContext dbContext,
        IValidator<CreateVesselAvailabilityCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<VesselAvailabilityRecordDto> HandleAsync(
        CreateVesselAvailabilityCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);

        var vesselId = VesselDomainHelper.ParseVesselId(command.VesselId);
        var vessel = await VesselDomainHelper.GetVesselOrThrowAsync(_dbContext, vesselId, cancellationToken);

        if (vessel.Status != VesselStatus.Active)
        {
            throw new VesselException(
                "Availability can only be added to active vessels.",
                StatusCodes.Status400BadRequest);
        }

        VesselDomainHelper.ValidateAvailabilityDateRange(command.AvailableFrom, command.AvailableTo);

        var activeWindows = await VesselDomainHelper.GetActiveAvailabilityWindowsAsync(
            _dbContext,
            vesselId,
            cancellationToken);

        try
        {
            VesselAvailabilityOverlapValidator.EnsureNoOverlap(
                command.AvailableFrom,
                command.AvailableTo,
                activeWindows);
        }
        catch (InvalidOperationException ex)
        {
            throw new VesselException(ex.Message, StatusCodes.Status409Conflict);
        }

        var availability = new VesselAvailability
        {
            VesselId = vesselId,
            AvailableFrom = command.AvailableFrom,
            AvailableTo = command.AvailableTo,
            OpenPort = command.OpenPort.Trim(),
            DestinationPort = command.DestinationPort?.Trim(),
            IsActive = true,
        };

        _dbContext.VesselAvailabilities.Add(availability);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return VesselMapper.ToRecordDto(availability);
    }
}
