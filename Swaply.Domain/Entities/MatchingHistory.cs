namespace Swaply.Domain.Entities;

public class MatchingHistory
{
    public Guid Id { get; private set; }
    public Guid SourceListingId { get; private set; }
    public Guid MatchedListingId { get; private set; }
    public decimal SimilarityScore { get; private set; }
    public bool IsAccepted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Listing? SourceListing { get; private set; }
    public Listing? MatchedListing { get; private set; }

    // EF Core constructor
    private MatchingHistory() { }

    public MatchingHistory(Guid sourceListingId, Guid matchedListingId, decimal similarityScore)
    {
        Id = Guid.NewGuid();
        SourceListingId = sourceListingId;
        MatchedListingId = matchedListingId;
        SimilarityScore = similarityScore;
        IsAccepted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void Accept()
    {
        IsAccepted = true;
    }

    public void Reject()
    {
        IsAccepted = false;
    }
}
