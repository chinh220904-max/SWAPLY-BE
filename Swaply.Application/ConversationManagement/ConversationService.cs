using Swaply.Application.NotificationManagement;
using Swaply.Domain.Entities;
using Swaply.Domain.Repositories;

namespace Swaply.Application.ConversationManagement;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public ConversationService(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IListingRepository listingRepository,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _listingRepository = listingRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<ConversationDto?> GetConversationByIdAsync(Guid conversationId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation == null) return null;

        if (!IsParticipant(conversation, currentUserId))
            return null;

        return ToConversationDto(conversation, currentUserId);
    }

    public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(userId, cancellationToken);
        return conversations.Select(c => ToConversationDto(c, userId));
    }

    public async Task<ConversationDto> CreateConversationAsync(Guid currentUserId, CreateConversationRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateConversationRequest(currentUserId, request, cancellationToken);

        var existing = await _conversationRepository.GetByUsersAndListingAsync(
            currentUserId, request.OtherUserId, request.RelatedListingId, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException("Conversation already exists for this listing.");

        var conversation = new Conversation(
            user1Id: currentUserId,
            user2Id: request.OtherUserId,
            relatedListingId: request.RelatedListingId,
            relatedExchangeId: request.RelatedExchangeId
        );

        var created = await _conversationRepository.CreateAsync(conversation, cancellationToken);

        return await GetConversationByIdAsync(created.Id, currentUserId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve created conversation.");
    }

    public async Task<ConversationDto?> GetOrCreateConversationAsync(Guid currentUserId, CreateConversationRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateConversationRequest(currentUserId, request, cancellationToken);

        var existing = await _conversationRepository.GetByUsersAndListingAsync(
            currentUserId, request.OtherUserId, request.RelatedListingId, cancellationToken);
        if (existing != null) return ToConversationDto(existing, currentUserId);

        var conversation = new Conversation(
            user1Id: currentUserId,
            user2Id: request.OtherUserId,
            relatedListingId: request.RelatedListingId,
            relatedExchangeId: request.RelatedExchangeId
        );

        var created = await _conversationRepository.CreateAsync(conversation, cancellationToken);
        return ToConversationDto(created, currentUserId);
    }

    public async Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(Guid conversationId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation == null)
            throw new InvalidOperationException("Conversation not found.");

        if (!IsParticipant(conversation, currentUserId))
            throw new UnauthorizedAccessException("You are not a participant in this conversation.");

        var messages = await _messageRepository.GetConversationMessagesAsync(conversationId, cancellationToken);
        return messages.Select(ToMessageDto);
    }

    public async Task<MessageDto> SendMessageAsync(Guid conversationId, Guid senderId, CreateMessageRequest request, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation == null)
            throw new InvalidOperationException("Conversation not found.");

        if (!IsParticipant(conversation, senderId))
            throw new UnauthorizedAccessException("You are not a participant in this conversation.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Message content cannot be empty.");

        var message = new Message(
            conversationId: conversationId,
            senderId: senderId,
            content: request.Content
        );

        conversation.UpdateLastMessageTime();

        var created = await _messageRepository.CreateAsync(message, cancellationToken);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        var receiverId = conversation.User1Id == senderId ? conversation.User2Id : conversation.User1Id;

        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest(
            UserId: receiverId,
            Title: "New Message",
            Content: "You received a new message.",
            Type: "NewMessage",
            RelatedEntityId: conversation.Id,
            RelatedEntityType: "Conversation"
        ), cancellationToken);

        return ToMessageDto(created);
    }

    private static bool IsParticipant(Conversation conversation, Guid userId)
    {
        return conversation.User1Id == userId || conversation.User2Id == userId;
    }

    private async Task ValidateConversationRequest(Guid currentUserId, CreateConversationRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUserId == request.OtherUserId)
            throw new ArgumentException("Cannot create a conversation with yourself.");

        var otherUser = await _userRepository.GetByIdAsync(request.OtherUserId);
        if (otherUser == null)
            throw new InvalidOperationException("Target user not found.");

        var listing = await _listingRepository.GetByIdAsync(request.RelatedListingId, cancellationToken);
        if (listing == null)
            throw new InvalidOperationException("Listing not found.");

        if (listing.OwnerId != request.OtherUserId)
            throw new InvalidOperationException("You can only start a conversation with the listing owner.");

        if (listing.OwnerId == currentUserId)
            throw new ArgumentException("Cannot create a conversation about your own listing.");
    }

    private static ConversationDto ToConversationDto(Conversation conversation, Guid currentUserId)
    {
        var otherUserId = conversation.User1Id == currentUserId ? conversation.User2Id : conversation.User1Id;
        var otherUser = conversation.User1Id == currentUserId ? conversation.User2 : conversation.User1;

        return new ConversationDto(
            Id: conversation.Id,
            OtherUserId: otherUserId,
            OtherUserName: otherUser?.UserName ?? "Unknown",
            OtherUserAvatarUrl: otherUser?.AvatarUrl,
            RelatedListingId: conversation.RelatedListingId,
            RelatedExchangeId: conversation.RelatedExchangeId,
            CreatedAt: conversation.CreatedAt,
            LastMessageAt: conversation.LastMessageAt
        );
    }

    private static MessageDto ToMessageDto(Message message)
    {
        return new MessageDto(
            Id: message.Id,
            ConversationId: message.ConversationId,
            SenderId: message.SenderId,
            SenderName: message.Sender?.UserName ?? "Unknown",
            Content: message.Content,
            IsRead: message.IsRead,
            CreatedAt: message.CreatedAt
        );
    }
}
