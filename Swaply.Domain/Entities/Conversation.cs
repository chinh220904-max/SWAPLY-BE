namespace Swaply.Domain.Entities;

public class Conversation
{
    public Guid Id { get; private set; }

    // Convention: User1Id and User2Id are set by the Application layer.
    // The Application layer is responsible for Guid normalization.
    public Guid User1Id { get; private set; }
    public Guid User2Id { get; private set; }

    public Guid? RelatedListingId { get; private set; }
    public Guid? RelatedExchangeId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastMessageAt { get; private set; }

    // Navigation properties (no reverse navigation from User to avoid ambiguous collection)
    public User? User1 { get; private set; }
    public User? User2 { get; private set; }
    public Listing? RelatedListing { get; private set; }
    public Exchange? RelatedExchange { get; private set; }

    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    // EF Core constructor
    private Conversation() { }

    public Conversation(Guid user1Id, Guid user2Id, Guid? relatedListingId = null, Guid? relatedExchangeId = null)
    {
        if (user1Id == user2Id)
            throw new ArgumentException("A conversation requires two different users.");

        Id = Guid.NewGuid();
        User1Id = user1Id;
        User2Id = user2Id;
        RelatedListingId = relatedListingId;
        RelatedExchangeId = relatedExchangeId;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateLastMessageTime()
    {
        LastMessageAt = DateTime.UtcNow;
    }
}
