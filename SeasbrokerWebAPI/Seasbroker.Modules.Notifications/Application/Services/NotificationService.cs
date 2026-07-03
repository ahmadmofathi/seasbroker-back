using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Notifications.Application.DTOs;
using Seasbroker.Modules.Notifications.Application.Exceptions;
using Seasbroker.Modules.Notifications.Application.Mapping;
using Seasbroker.Modules.Notifications.Application.Queries;

namespace Seasbroker.Modules.Notifications.Application.Services;

public interface INotificationService
{
    Task<NotificationDto> CreateAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDto>> CreateManyAsync(
        IEnumerable<CreateNotificationRequest> requests,
        CancellationToken cancellationToken = default);

    Task<NotificationListResponse> GetForUserAsync(
        GetNotificationsQuery query,
        CancellationToken cancellationToken = default);

    Task<NotificationListResponse> GetUnreadForUserAsync(
        GetUnreadNotificationsQuery query,
        CancellationToken cancellationToken = default);

    Task<NotificationDto> MarkReadAsync(
        MarkNotificationReadCommand command,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllReadAsync(
        MarkAllNotificationsReadCommand command,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        DeleteNotificationCommand command,
        CancellationToken cancellationToken = default);
}

public class NotificationService : INotificationService
{
    private readonly SeasbrokerDbContext _dbContext;

    public NotificationService(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationDto> CreateAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var results = await CreateManyAsync([request], cancellationToken);
        return results[0];
    }

    public async Task<IReadOnlyList<NotificationDto>> CreateManyAsync(
        IEnumerable<CreateNotificationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var entities = requests.Select(request => new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            NotificationType = request.NotificationType,
            Status = NotificationStatus.Unread,
            CreatedAt = utcNow,
            Payload = request.Payload,
        }).ToList();

        if (entities.Count == 0)
        {
            return Array.Empty<NotificationDto>();
        }

        _dbContext.Notifications.AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entities.Select(NotificationMapper.ToDto).ToList();
    }

    public async Task<NotificationListResponse> GetForUserAsync(
        GetNotificationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var perPage = query.PerPage < 1 ? 50 : Math.Min(query.PerPage, 200);

        var notificationsQuery = _dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == query.UserId);

        return await BuildListResponseAsync(notificationsQuery, page, perPage, cancellationToken);
    }

    public async Task<NotificationListResponse> GetUnreadForUserAsync(
        GetUnreadNotificationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var perPage = query.PerPage < 1 ? 50 : Math.Min(query.PerPage, 200);

        var notificationsQuery = _dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == query.UserId && n.Status == NotificationStatus.Unread);

        return await BuildListResponseAsync(notificationsQuery, page, perPage, cancellationToken);
    }

    public async Task<NotificationDto> MarkReadAsync(
        MarkNotificationReadCommand command,
        CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                n => n.Id == command.NotificationId && n.UserId == command.UserId,
                cancellationToken);

        if (notification is null)
        {
            throw new NotificationException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        if (notification.Status == NotificationStatus.Unread)
        {
            notification.Status = NotificationStatus.Read;
            notification.ReadAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return NotificationMapper.ToDto(notification);
    }

    public async Task<int> MarkAllReadAsync(
        MarkAllNotificationsReadCommand command,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var unread = await _dbContext.Notifications
            .Where(n => n.UserId == command.UserId && n.Status == NotificationStatus.Unread)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
        {
            notification.Status = NotificationStatus.Read;
            notification.ReadAt = utcNow;
        }

        if (unread.Count == 0)
        {
            return 0;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return unread.Count;
    }

    public async Task DeleteAsync(
        DeleteNotificationCommand command,
        CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                n => n.Id == command.NotificationId && n.UserId == command.UserId,
                cancellationToken);

        if (notification is null)
        {
            throw new NotificationException("The requested resource wasn't found.", StatusCodes.Status404NotFound);
        }

        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<NotificationListResponse> BuildListResponseAsync(
        IQueryable<Notification> query,
        int page,
        int perPage,
        CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)perPage);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(cancellationToken);

        return new NotificationListResponse
        {
            Page = page,
            PerPage = perPage,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = items.Select(NotificationMapper.ToDto).ToList(),
        };
    }
}
