using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Notifications.Application.Queries;
using Seasbroker.Modules.Notifications.Application.Services;

namespace Seasbroker.Modules.Notifications.Application.Services;

public interface INotificationDispatcher
{
    Task DispatchAsync(
        IEnumerable<CreateNotificationRequest> requests,
        CancellationToken cancellationToken = default);
}

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly INotificationService _notificationService;
    private readonly ISignalRNotificationService _signalRNotificationService;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        INotificationService notificationService,
        ISignalRNotificationService signalRNotificationService,
        ILogger<NotificationDispatcher> logger)
    {
        _notificationService = notificationService;
        _signalRNotificationService = signalRNotificationService;
        _logger = logger;
    }

    public async Task DispatchAsync(
        IEnumerable<CreateNotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var requestList = requests.ToList();
        if (requestList.Count == 0)
        {
            return;
        }

        IReadOnlyList<Application.DTOs.NotificationDto> persisted;

        try
        {
            persisted = await _notificationService.CreateManyAsync(requestList, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist notifications.");
            throw;
        }

        try
        {
            await _signalRNotificationService.PushManyAsync(persisted, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR broadcast failed after notifications were persisted.");
        }
    }
}

public interface INotificationRecipientResolver
{
    Task<IReadOnlyList<Guid>> ResolveSuperuserIdsAsync(CancellationToken cancellationToken = default);

    Task<(Guid CargoOwnerId, Guid? VesselOwnerId)> ResolveMatchOwnersAsync(
        Guid cargoListingId,
        Guid vesselId,
        CancellationToken cancellationToken = default);
}

public class NotificationRecipientResolver : INotificationRecipientResolver
{
    private readonly SeasbrokerDbContext _dbContext;

    public NotificationRecipientResolver(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Guid>> ResolveSuperuserIdsAsync(CancellationToken cancellationToken = default)
    {
        var superuserRoleId = await _dbContext.Roles
            .AsNoTracking()
            .Where(r => r.Name == "Superuser")
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (superuserRoleId == Guid.Empty)
        {
            return Array.Empty<Guid>();
        }

        return await _dbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.RoleId == superuserRoleId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<(Guid CargoOwnerId, Guid? VesselOwnerId)> ResolveMatchOwnersAsync(
        Guid cargoListingId,
        Guid vesselId,
        CancellationToken cancellationToken = default)
    {
        var cargoOwnerId = await _dbContext.CargoListings
            .AsNoTracking()
            .Where(c => c.Id == cargoListingId)
            .Select(c => c.CustomerId)
            .FirstAsync(cancellationToken);

        var vesselOwnerId = await _dbContext.Vessels
            .AsNoTracking()
            .Where(v => v.Id == vesselId)
            .Select(v => v.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        return (cargoOwnerId, vesselOwnerId);
    }
}

public static class NotificationPayloadBuilder
{
    public static string Build(object payload) =>
        JsonSerializer.Serialize(payload);
}
