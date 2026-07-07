namespace Swaply.Domain.Entities;

public class Message
{
    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public Conversation? Conversation { get; private set; }
    public User? Sender { get; private set; }

    // EF Core constructor
    private Message() { }

    public Message(Guid conversationId, Guid senderId, string content)
    {
        Id = Guid.NewGuid();
        ConversationId = conversationId;
        SenderId = senderId;
        Content = content;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
