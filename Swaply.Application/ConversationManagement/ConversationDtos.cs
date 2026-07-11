namespace Swaply.Application.ConversationManagement;

public record ConversationDto(
    Guid Id,
    Guid OtherUserId,
    string OtherUserName,
    string? OtherUserAvatarUrl,
    Guid? RelatedListingId,
    Guid? RelatedExchangeId,
    DateTime CreatedAt,
    DateTime? LastMessageAt
);

public record ConversationDetailDto(
    Guid Id,
    Guid User1Id,
    string User1Name,
    string? User1AvatarUrl,
    Guid User2Id,
    string User2Name,
    string? User2AvatarUrl,
    Guid? RelatedListingId,
    Guid? RelatedExchangeId,
    DateTime CreatedAt,
    DateTime? LastMessageAt
);

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string SenderName,
    string Content,
    bool IsRead,
    DateTime CreatedAt
);

public record CreateConversationRequest(
    Guid OtherUserId,
    Guid RelatedListingId,
    Guid? RelatedExchangeId = null
);

public record CreateMessageRequest(
    string Content
);
