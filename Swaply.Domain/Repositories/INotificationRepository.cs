using Swaply.Domain.Entities;

namespace Swaply.Domain.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
