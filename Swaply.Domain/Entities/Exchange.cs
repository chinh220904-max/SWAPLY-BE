using Swaply.Domain.Enums;

namespace Swaply.Domain.Entities;

public class Exchange
{
    public Guid Id { get; private set; }
    public Guid ProposerListingId { get; private set; }
    public Guid ReceiverListingId { get; private set; }
    public string ProposerId { get; private set; } = string.Empty;
    public string ReceiverId { get; private set; } = string.Empty;
    public ExchangeStatus Status { get; private set; } = ExchangeStatus.Pending;
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    private Exchange() { }

    public Exchange(Guid id, Guid proposerListingId, Guid receiverListingId, string proposerId, string receiverId)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ProposerListingId = proposerListingId;
        ReceiverListingId = receiverListingId;
        ProposerId = proposerId;
        ReceiverId = receiverId;
        Status = ExchangeStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Accept()
    {
        if (Status != ExchangeStatus.Pending)
        {
            throw new InvalidOperationException("Only pending exchanges can be accepted.");
        }
        Status = ExchangeStatus.Accepted;
    }

    public void Reject()
    {
        if (Status != ExchangeStatus.Pending)
        {
            throw new InvalidOperationException("Only pending exchanges can be rejected.");
        }
        Status = ExchangeStatus.Rejected;
    }

    public void Complete()
    {
        if (Status != ExchangeStatus.Accepted)
        {
            throw new InvalidOperationException("Exchange must be accepted before completing.");
        }
        Status = ExchangeStatus.Completed;
    }
}
