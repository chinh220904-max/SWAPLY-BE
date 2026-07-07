namespace Swaply.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string IconUrl { get; private set; } = string.Empty;

    private readonly List<Listing> _listings = new();
    public IReadOnlyCollection<Listing> Listings => _listings.AsReadOnly();

    private Category() { }

    public Category(Guid id, string name, string description, string iconUrl)
    {
        Id = id;
        Name = name;
        Description = description;
        IconUrl = iconUrl;
    }
}
