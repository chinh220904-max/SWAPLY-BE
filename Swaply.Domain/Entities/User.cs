namespace Swaply.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? FullName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? PhoneNumber { get; private set; }
    public Guid RoleId { get; private set; }

    // Navigation property
    public Role? Role { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsBanned { get; private set; }
    public DateTime? BannedAt { get; private set; }
    public string? BanReason { get; private set; }

    // Navigation properties
    // Note: User -> Conversation reverse navigation is intentionally omitted.
    // Conversation uses User1Id/User2Id. A single User.Conversations collection
    // would be ambiguous. Use ConversationRepository.GetByUserIdAsync() instead.

    private readonly List<Listing> _listings = new();
    public IReadOnlyCollection<Listing> Listings => _listings.AsReadOnly();

    private readonly List<Exchange> _proposedExchanges = new();
    public IReadOnlyCollection<Exchange> ProposedExchanges => _proposedExchanges.AsReadOnly();

    private readonly List<Exchange> _receivedExchanges = new();
    public IReadOnlyCollection<Exchange> ReceivedExchanges => _receivedExchanges.AsReadOnly();

    private readonly List<Review> _reviewsGiven = new();
    public IReadOnlyCollection<Review> ReviewsGiven => _reviewsGiven.AsReadOnly();

    private readonly List<Review> _reviewsReceived = new();
    public IReadOnlyCollection<Review> ReviewsReceived => _reviewsReceived.AsReadOnly();

    private readonly List<Report> _reportsMade = new();
    public IReadOnlyCollection<Report> ReportsMade => _reportsMade.AsReadOnly();

    private readonly List<Favorite> _favorites = new();
    public IReadOnlyCollection<Favorite> Favorites => _favorites.AsReadOnly();

    // EF Core constructor
    private User() { }

    public User(string email, string userName, string passwordHash, string? phoneNumber, Guid roleId, string? fullName = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username is required.", nameof(userName));

        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant().Trim();
        UserName = userName.Trim();
        PasswordHash = passwordHash;
        PhoneNumber = phoneNumber?.Trim();
        RoleId = roleId;
        FullName = fullName?.Trim();
        CreatedAt = DateTime.UtcNow;
        IsBanned = false;
    }

    public void SetPhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber?.Trim();
    }

    public void UpdateProfile(string? fullName, string? phoneNumber)
    {
        FullName = fullName?.Trim();
        PhoneNumber = phoneNumber?.Trim();
    }

    public void SetAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
    }

    public void SetRole(Guid roleId)
    {
        RoleId = roleId;
    }

    public void Ban(string reason)
    {
        IsBanned = true;
        BannedAt = DateTime.UtcNow;
        BanReason = reason;
    }

    public void Unban()
    {
        IsBanned = false;
        BannedAt = null;
        BanReason = null;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
