using Microsoft.EntityFrameworkCore;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence;

public class SwaplyDbContext : DbContext
{
    public SwaplyDbContext(DbContextOptions<SwaplyDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<Exchange> Exchanges => Set<Exchange>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Swaply.Domain.Entities.Payment> Payments => Set<Swaply.Domain.Entities.Payment>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<MatchingHistory> MatchingHistories => Set<MatchingHistory>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SwaplyDbContext).Assembly);
    }
}
