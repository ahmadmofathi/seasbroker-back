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

public class UpdateVesselCommandHandler : ICommandHandler<UpdateVesselCommand, VesselRecordDto>
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IValidator<UpdateVesselCommand> _validator;

    public UpdateVesselCommandHandler(
        SeasbrokerDbContext dbContext,
        IValidator<UpdateVesselCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<VesselRecordDto> HandleAsync(
        UpdateVesselCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);

        var vesselId = VesselDomainHelper.ParseVesselId(command.VesselId);
        var vessel = await VesselDomainHelper.GetVesselOrThrowAsync(_dbContext, vesselId, cancellationToken);

        if (command.Name is not null)
        {
            vessel.Name = command.Name.Trim();
        }

        if (command.VesselType is not null)
        {
            vessel.VesselType = command.VesselType;
        }

        if (command.Dwt.HasValue)
        {
            vessel.Dwt = command.Dwt.Value;
        }

        if (command.TeuCapacity.HasValue)
        {
            vessel.TeuCapacity = command.TeuCapacity;
        }

        if (command.LengthOverall.HasValue)
        {
            vessel.LengthOverall = command.LengthOverall;
        }

        if (command.Beam.HasValue)
        {
            vessel.Beam = command.Beam;
        }

        if (command.Draft.HasValue)
        {
            vessel.Draft = command.Draft;
        }

        if (command.CurrentPort is not null)
        {
            vessel.CurrentPort = command.CurrentPort.Trim();
        }

        if (command.FlagCountry is not null)
        {
            vessel.FlagCountry = command.FlagCountry.Trim();
        }

        if (command.Status is not null)
        {
            vessel.Status = command.Status;
        }

        if (command.Notes is not null)
        {
            vessel.Notes = command.Notes.Trim();
        }

        if (command.CustomerId is not null)
        {
            var customerId = VesselDomainHelper.ParseOptionalCustomerId(command.CustomerId);

            if (customerId.HasValue)
            {
                await VesselDomainHelper.EnsureCustomerExistsAsync(_dbContext, customerId.Value, cancellationToken);
            }

            vessel.CustomerId = customerId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return VesselMapper.ToRecordDto(vessel);
    }
}
