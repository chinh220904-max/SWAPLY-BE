using Swaply.Domain.Enums;

namespace Swaply.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public NotificationEntityType RelatedEntityType { get; private set; } = NotificationEntityType.Unspecified;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    private Notification() { }

    public Notification(Guid userId, string title, string content, NotificationType type,
        Guid? relatedEntityId = null, NotificationEntityType relatedEntityType = NotificationEntityType.Unspecified)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        Content = content;
        Type = type;
        RelatedEntityId = relatedEntityId;
        RelatedEntityType = relatedEntityType;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
