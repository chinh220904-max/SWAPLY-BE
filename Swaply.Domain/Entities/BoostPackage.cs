namespace Swaply.Domain.Entities;

public class BoostPackage
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int DurationDays { get; private set; }
    public int MaxListings { get; private set; }
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<BoostSubscription> _subscriptions = new();
    public IReadOnlyCollection<BoostSubscription> Subscriptions => _subscriptions.AsReadOnly();

    private BoostPackage() { }

    public BoostPackage(
        string name,
        string description,
        decimal price,
        int durationDays,
        int maxListings,
        int priority = 100)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (durationDays <= 0)
            throw new ArgumentException("Duration must be positive.", nameof(durationDays));
        if (maxListings <= 0)
            throw new ArgumentException("MaxListings must be positive.", nameof(maxListings));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        DurationDays = durationDays;
        MaxListings = maxListings;
        Priority = priority;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string description,
        decimal price,
        int durationDays,
        int maxListings,
        int priority,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (durationDays <= 0)
            throw new ArgumentException("Duration must be positive.", nameof(durationDays));
        if (maxListings <= 0)
            throw new ArgumentException("MaxListings must be positive.", nameof(maxListings));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        Name = name;
        Description = description;
        Price = price;
        DurationDays = durationDays;
        MaxListings = maxListings;
        Priority = priority;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
