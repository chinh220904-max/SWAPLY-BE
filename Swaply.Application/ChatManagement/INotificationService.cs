namespace Swaply.Application.ChatManagement;

public interface INotificationService
{
    Task SendNotificationToUserAsync(string userId, string message, CancellationToken cancellationToken = default);
}
