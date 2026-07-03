using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.Helpers;

namespace Seasbroker.Modules.Vessel.Application.Handlers.Commands;

public class DeactivateVesselCommandHandler : ICommandHandler<DeactivateVesselCommand>
{
    private readonly SeasbrokerDbContext _dbContext;

    public DeactivateVesselCommandHandler(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(DeactivateVesselCommand command, CancellationToken cancellationToken = default)
    {
        var vesselId = VesselDomainHelper.ParseVesselId(command.VesselId);
        var vessel = await VesselDomainHelper.GetVesselOrThrowAsync(_dbContext, vesselId, cancellationToken);

        vessel.Status = VesselStatus.Inactive;

        var activeAvailabilities = await _dbContext.VesselAvailabilities
            .Where(a => a.VesselId == vesselId && a.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var availability in activeAvailabilities)
        {
            availability.IsActive = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
