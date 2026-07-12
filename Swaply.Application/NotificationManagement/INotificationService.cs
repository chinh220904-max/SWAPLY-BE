namespace Swaply.Application.NotificationManagement;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    Task<NotificationDto> MarkAsReadAsync(Guid notificationId, Guid currentUserId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
