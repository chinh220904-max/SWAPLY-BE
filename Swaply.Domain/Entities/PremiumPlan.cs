namespace Swaply.Domain.Entities;

public class PremiumPlan
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int DurationDays { get; private set; }
    public int MaxListings { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Subscription> _subscriptions = new();
    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    // EF Core constructor
    private PremiumPlan() { }

    public PremiumPlan(Guid id, string name, string description, decimal price, int durationDays, int maxListings)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        DurationDays = durationDays;
        MaxListings = maxListings;
        IsActive = true;
    }
}
