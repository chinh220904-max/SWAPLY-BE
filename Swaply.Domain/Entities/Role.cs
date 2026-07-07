namespace Swaply.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    // EF Core constructor
    private Role() { }

    public Role(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}
