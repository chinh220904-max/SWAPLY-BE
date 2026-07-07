namespace Swaply.Domain.Entities;

public class Exchange
{
    public Guid Id { get; private set; }
    public Guid ProposerListingId { get; private set; }
    public Guid ReceiverListingId { get; private set; }
    public Guid ProposerId { get; private set; }
    public Guid ReceiverId { get; private set; }
    public ExchangeStatus Status { get; private set; } = ExchangeStatus.Pending;
    public string? Message { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public Listing? ProposerListing { get; private set; }
    public Listing? ReceiverListing { get; private set; }
    public User? Proposer { get; private set; }
    public User? Receiver { get; private set; }

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    // EF Core constructor
    private Exchange() { }

    public Exchange(Guid proposerListingId, Guid receiverListingId, Guid proposerId, Guid receiverId, string? message = null)
    {
        if (proposerId == receiverId)
            throw new ArgumentException("User cannot propose an exchange to themselves.");

        Id = Guid.NewGuid();
        ProposerListingId = proposerListingId;
        ReceiverListingId = receiverListingId;
        ProposerId = proposerId;
        ReceiverId = receiverId;
        Message = message;
        Status = ExchangeStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Accept()
    {
        if (Status != ExchangeStatus.Pending)
            throw new InvalidOperationException("Only pending exchanges can be accepted.");
        Status = ExchangeStatus.Accepted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != ExchangeStatus.Pending)
            throw new InvalidOperationException("Only pending exchanges can be rejected.");
        Status = ExchangeStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != ExchangeStatus.Accepted)
            throw new InvalidOperationException("Exchange must be accepted before completing.");
        Status = ExchangeStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ExchangeStatus.Completed)
            throw new InvalidOperationException("Completed exchanges cannot be cancelled.");
        Status = ExchangeStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
