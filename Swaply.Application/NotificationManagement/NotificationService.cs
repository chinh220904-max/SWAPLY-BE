using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;

namespace Swaply.Application.NotificationManagement;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IRealTimeNotificationService _realTimeNotificationService;

    public NotificationService(
        INotificationRepository notificationRepository,
        IRealTimeNotificationService realTimeNotificationService)
    {
        _notificationRepository = notificationRepository;
        _realTimeNotificationService = realTimeNotificationService;
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, cancellationToken);
        return notifications.Select(ToNotificationDto);
    }

    public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId, cancellationToken);
        return notifications.Select(ToNotificationDto);
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var count = await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
        return new UnreadCountDto(count);
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required.");

        if (request.Title.Length > 200)
            throw new ArgumentException("Title must not exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Content is required.");

        if (request.Content.Length > 2000)
            throw new ArgumentException("Content must not exceed 2000 characters.");

        if (!Enum.TryParse<NotificationType>(request.Type, true, out var notificationType))
            throw new ArgumentException("Invalid notification type.");

        var relatedEntityType = Enum.TryParse<NotificationEntityType>(request.RelatedEntityType, true, out var entityType)
            ? entityType
            : NotificationEntityType.Unspecified;

        var notification = new Notification(
            userId: request.UserId,
            title: request.Title,
            content: request.Content,
            type: notificationType,
            relatedEntityId: request.RelatedEntityId,
            relatedEntityType: relatedEntityType
        );

        var created = await _notificationRepository.CreateAsync(notification, cancellationToken);
        var notificationDto = ToNotificationDto(created);

        await _realTimeNotificationService.SendNotificationToUserAsync(
            created.UserId.ToString(),
            notificationDto,
            cancellationToken);

        return notificationDto;
    }

    public async Task<NotificationDto> MarkAsReadAsync(Guid notificationId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);

        if (notification == null)
            throw new InvalidOperationException("Notification not found.");

        if (notification.UserId != currentUserId)
            throw new UnauthorizedAccessException("You can only mark your own notifications as read.");

        notification.MarkAsRead();
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        return ToNotificationDto(notification);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId, cancellationToken);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto ToNotificationDto(Notification notification)
    {
        return new NotificationDto(
            Id: notification.Id,
            UserId: notification.UserId,
            Title: notification.Title,
            Content: notification.Content,
            Type: notification.Type.ToString(),
            RelatedEntityId: notification.RelatedEntityId,
            RelatedEntityType: notification.RelatedEntityType.ToString(),
            IsRead: notification.IsRead,
            CreatedAt: notification.CreatedAt
        );
    }
}
