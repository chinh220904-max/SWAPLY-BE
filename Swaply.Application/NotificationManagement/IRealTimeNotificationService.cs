namespace Swaply.Application.NotificationManagement;

public interface IRealTimeNotificationService
{
    Task SendNotificationToUserAsync(string userId, NotificationDto notification, CancellationToken cancellationToken = default);
}
