using Microsoft.AspNetCore.Http;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Exceptions;
using Seasbroker.Modules.Vessel.Application.Helpers;

namespace Seasbroker.Modules.Vessel.Application.Handlers.Commands;

public class DeactivateVesselAvailabilityCommandHandler : ICommandHandler<DeactivateVesselAvailabilityCommand>
{
    private readonly SeasbrokerDbContext _dbContext;

    public DeactivateVesselAvailabilityCommandHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        DeactivateVesselAvailabilityCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(command.AvailabilityId, out var availabilityId))
        {
            throw new VesselException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        var availability = await VesselDomainHelper.GetAvailabilityOrThrowAsync(
            _dbContext,
            availabilityId,
            cancellationToken);

        availability.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
