using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Approval.Application.Events;
using Seasbroker.Modules.Matching.Application.Events;
using Seasbroker.Modules.Notifications.Application.EventHandlers;
using Seasbroker.Modules.Notifications.Application.Queries;
using Seasbroker.Modules.Notifications.Application.Services;
using Seasbroker.Modules.Notifications.Infrastructure;

namespace Seasbroker.Modules.Notifications.Tests;

public class NotificationServiceTests
{
    [Fact]
    public async Task CreateManyAsync_PersistsNotifications()
    {
        await using var dbContext = CreateDbContext();
        var service = new NotificationService(dbContext);
        var userId = Guid.NewGuid();

        var created = await service.CreateManyAsync([
            new CreateNotificationRequest(
                userId,
                "Test",
                "Message body",
                NotificationType.SystemNotification,
                "{\"key\":\"value\"}"),
        ]);

        Assert.Single(created);
        Assert.Equal(NotificationStatus.Unread, created[0].Status);
        Assert.Equal(1, await dbContext.Notifications.CountAsync());
    }

    [Fact]
    public async Task MarkAllReadAsync_UpdatesUnreadNotifications()
    {
        await using var dbContext = CreateDbContext();
        var service = new NotificationService(dbContext);
        var userId = Guid.NewGuid();

        dbContext.Notifications.AddRange(
            new Notification
            {
                UserId = userId,
                Title = "A",
                Message = "A",
                NotificationType = NotificationType.SystemNotification,
                Status = NotificationStatus.Unread,
                CreatedAt = DateTime.UtcNow,
            },
            new Notification
            {
                UserId = userId,
                Title = "B",
                Message = "B",
                NotificationType = NotificationType.SystemNotification,
                Status = NotificationStatus.Read,
                CreatedAt = DateTime.UtcNow,
                ReadAt = DateTime.UtcNow,
            });

        await dbContext.SaveChangesAsync();

        var updated = await service.MarkAllReadAsync(new MarkAllNotificationsReadCommand(userId));
        Assert.Equal(1, updated);
    }

    private static SeasbrokerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SeasbrokerDbContext(options);
    }
}

public class MatchNotificationEventHandlerTests
{
    [Fact]
    public async Task HandlePendingApprovalAsync_CreatesNotificationsForOwnersAndSuperusers()
    {
        await using var dbContext = CreateDbContext();
        var (cargoListingId, vesselId, cargoOwnerId, vesselOwnerId, superuserId) =
            await SeedMatchContextAsync(dbContext);

        var handler = CreateHandler(dbContext);

        await handler.HandlePendingApprovalAsync(
            new MatchPendingApprovalEvent(
                Guid.NewGuid(),
                cargoListingId,
                vesselId,
                90m,
                MatchSource.Automatic));

        var notifications = await dbContext.Notifications.ToListAsync();
        Assert.Equal(3, notifications.Count);
        Assert.Contains(notifications, n => n.UserId == cargoOwnerId);
        Assert.Contains(notifications, n => n.UserId == vesselOwnerId);
        Assert.Contains(notifications, n => n.UserId == superuserId);
        Assert.All(notifications, n => Assert.Equal(NotificationType.MatchPendingApproval, n.NotificationType));
    }

    [Fact]
    public async Task DispatchAsync_PersistsEvenWhenSignalRFails()
    {
        await using var dbContext = CreateDbContext();
        var notificationService = new NotificationService(dbContext);
        var dispatcher = new NotificationDispatcher(
            notificationService,
            new FailingSignalRNotificationService(),
            NullLogger<NotificationDispatcher>.Instance);

        await dispatcher.DispatchAsync([
            new CreateNotificationRequest(
                Guid.NewGuid(),
                "Title",
                "Body",
                NotificationType.SystemNotification,
                null),
        ]);

        Assert.Equal(1, await dbContext.Notifications.CountAsync());
    }

    [Fact]
    public async Task MatchingDomainEventBridge_ForwardsPendingApprovalEvent()
    {
        await using var dbContext = CreateDbContext();
        var (cargoListingId, vesselId, _, _, _) = await SeedMatchContextAsync(dbContext);
        var handler = CreateHandler(dbContext);
        var bridge = new MatchingDomainEventBridge(handler);

        await bridge.PublishAsync(
            new MatchPendingApprovalEvent(Guid.NewGuid(), cargoListingId, vesselId, 80m, MatchSource.Manual));

        Assert.True(await dbContext.Notifications.AnyAsync());
    }

    [Fact]
    public async Task ApprovalDomainEventBridge_ForwardsApprovedEvent()
    {
        await using var dbContext = CreateDbContext();
        var (cargoListingId, vesselId, _, _, _) = await SeedMatchContextAsync(dbContext);
        var handler = CreateHandler(dbContext);
        var bridge = new ApprovalDomainEventBridge(handler);

        await bridge.PublishAsync(
            new MatchApprovedEvent(Guid.NewGuid(), cargoListingId, vesselId, Guid.NewGuid(), 95m));

        Assert.All(
            await dbContext.Notifications.ToListAsync(),
            n => Assert.Equal(NotificationType.MatchApproved, n.NotificationType));
    }

    private static MatchNotificationEventHandler CreateHandler(SeasbrokerDbContext dbContext)
    {
        var notificationService = new NotificationService(dbContext);
        var dispatcher = new NotificationDispatcher(
            notificationService,
            new NoOpSignalRNotificationService(),
            NullLogger<NotificationDispatcher>.Instance);

        return new MatchNotificationEventHandler(
            dispatcher,
            new NotificationRecipientResolver(dbContext));
    }

    private static async Task<(Guid CargoListingId, Guid VesselId, Guid CargoOwnerId, Guid VesselOwnerId, Guid SuperuserId)>
        SeedMatchContextAsync(SeasbrokerDbContext dbContext)
    {
        var cargoOwnerId = Guid.NewGuid();
        var vesselOwnerId = Guid.NewGuid();
        var superuserId = Guid.NewGuid();
        var superuserRoleId = Guid.NewGuid();
        var cargoListingId = Guid.NewGuid();
        var vesselId = Guid.NewGuid();
        var departure = DateTime.UtcNow;
        var arrival = departure.AddDays(5);

        dbContext.Customers.AddRange(
            new Customer
            {
                Id = cargoOwnerId,
                Email = "cargo@example.com",
                PhoneNumber = "1",
                FirstName = "Cargo",
                LastName = "Owner",
            },
            new Customer
            {
                Id = vesselOwnerId,
                Email = "vessel@example.com",
                PhoneNumber = "2",
                FirstName = "Vessel",
                LastName = "Owner",
            });

        dbContext.Roles.Add(new Role
        {
            Id = superuserRoleId,
            Name = "Superuser",
            NormalizedName = "SUPERUSER",
        });

        dbContext.Users.Add(new User
        {
            Id = superuserId,
            Email = "admin@example.com",
            UserName = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            NormalizedUserName = "ADMIN@EXAMPLE.COM",
        });

        dbContext.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
        {
            UserId = superuserId,
            RoleId = superuserRoleId,
        });

        dbContext.CargoListings.Add(new CargoListing
        {
            Id = cargoListingId,
            CustomerId = cargoOwnerId,
            ReferenceNumber = "CRG-NOTIFY-001",
            CargoType = "Bulk",
            Weight = 1000,
            Dimensions = "1x1x1",
            DeparturePort = "A",
            ArrivalPort = "B",
            DepartureTime = departure,
            ArrivalTime = arrival,
            Status = CargoStatus.Open,
        });

        dbContext.Vessels.Add(new Vessel
        {
            Id = vesselId,
            Name = "Notify Vessel",
            VesselType = "Bulk",
            Dwt = 5000,
            CurrentPort = "A",
            CustomerId = vesselOwnerId,
            Status = VesselStatus.Active,
        });

        await dbContext.SaveChangesAsync();
        return (cargoListingId, vesselId, cargoOwnerId, vesselOwnerId, superuserId);
    }

    private static SeasbrokerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SeasbrokerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SeasbrokerDbContext(options);
    }

    private sealed class NoOpSignalRNotificationService : ISignalRNotificationService
    {
        public Task PushAsync(Application.DTOs.NotificationDto notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task PushManyAsync(
            IReadOnlyList<Application.DTOs.NotificationDto> notifications,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FailingSignalRNotificationService : ISignalRNotificationService
    {
        public Task PushAsync(Application.DTOs.NotificationDto notification, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("SignalR unavailable");

        public Task PushManyAsync(
            IReadOnlyList<Application.DTOs.NotificationDto> notifications,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("SignalR unavailable");
    }
}
