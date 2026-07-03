using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Helpers;
using Seasbroker.Modules.Vessel.Application.Mapping;
using Seasbroker.Modules.Vessel.Application.Validators;

namespace Seasbroker.Modules.Vessel.Application.Handlers.Commands;

public class CreateVesselCommandHandler : ICommandHandler<CreateVesselCommand, VesselRecordDto>
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IValidator<CreateVesselCommand> _validator;

    public CreateVesselCommandHandler(
        SeasbrokerDbContext dbContext,
        IValidator<CreateVesselCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<VesselRecordDto> HandleAsync(
        CreateVesselCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);

        var customerId = VesselDomainHelper.ParseOptionalCustomerId(command.CustomerId);

        if (customerId.HasValue)
        {
            await VesselDomainHelper.EnsureCustomerExistsAsync(_dbContext, customerId.Value, cancellationToken);
        }

        await VesselDomainHelper.EnsureImoUniqueAsync(_dbContext, command.ImoNumber, null, cancellationToken);

        var vessel = new global::Seasbroker.Infrastructure.Persistence.Entities.Vessel
        {
            Name = command.Name.Trim(),
            ImoNumber = string.IsNullOrWhiteSpace(command.ImoNumber) ? null : command.ImoNumber.Trim(),
            VesselType = command.VesselType,
            Dwt = command.Dwt,
            TeuCapacity = command.TeuCapacity,
            LengthOverall = command.LengthOverall,
            Beam = command.Beam,
            Draft = command.Draft,
            CurrentPort = command.CurrentPort.Trim(),
            FlagCountry = command.FlagCountry?.Trim(),
            Status = string.IsNullOrWhiteSpace(command.Status) ? VesselStatus.Active : command.Status,
            CustomerId = customerId,
            Notes = command.Notes?.Trim(),
        };

        _dbContext.Vessels.Add(vessel);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return VesselMapper.ToRecordDto(vessel);
    }
}
