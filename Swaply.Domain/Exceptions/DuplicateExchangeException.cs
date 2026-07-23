using Swaply.Domain.Exceptions;

namespace Swaply.Domain.Exceptions;

public class DuplicateExchangeException : DomainException
{
    public Guid? ExistingExchangeId { get; }
    public Guid Listing1Id { get; }
    public Guid Listing2Id { get; }
    public bool IsReversed { get; }

    public DuplicateExchangeException(
        string message,
        Guid listing1Id,
        Guid listing2Id,
        bool isReversed = false,
        Guid? existingExchangeId = null)
        : base(message)
    {
        Listing1Id = listing1Id;
        Listing2Id = listing2Id;
        IsReversed = isReversed;
        ExistingExchangeId = existingExchangeId;
    }

    public DuplicateExchangeException(
        string message,
        Guid listing1Id,
        Guid listing2Id,
        bool isReversed,
        Guid existingExchangeId)
        : this(message, listing1Id, listing2Id, isReversed, (Guid?)existingExchangeId)
    {
    }
}
