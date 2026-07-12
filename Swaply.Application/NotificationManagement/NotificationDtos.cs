namespace Swaply.Application.NotificationManagement;

public record NotificationDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Content,
    string Type,
    Guid? RelatedEntityId,
    string RelatedEntityType,
    bool IsRead,
    DateTime CreatedAt
);

public record CreateNotificationRequest(
    Guid UserId,
    string Title,
    string Content,
    string Type,
    Guid? RelatedEntityId = null,
    string RelatedEntityType = "Unspecified"
);

public record UnreadCountDto(int Count);
