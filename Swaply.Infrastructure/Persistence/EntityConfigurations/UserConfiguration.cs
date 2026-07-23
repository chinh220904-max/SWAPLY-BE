using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.UserName)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.FullName)
            .HasMaxLength(200);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(u => u.BanReason)
            .HasMaxLength(1000);

        builder.Property(u => u.IsBanned)
            .HasDefaultValue(false);

        // Role relationship (1:N)
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // User -> Listing (1:N) with Restrict to avoid multiple cascade paths
        // (Favorite also cascades from User -> Listing, creating a dual cascade path)
        builder.HasMany(u => u.Listings)
            .WithOne(l => l.Owner)
            .HasForeignKey(l => l.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Favorites)
            .WithOne(f => f.User)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ProposedExchanges)
            .WithOne(e => e.Proposer)
            .HasForeignKey(e => e.ProposerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ReceivedExchanges)
            .WithOne(e => e.Receiver)
            .HasForeignKey(e => e.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ReviewsGiven)
            .WithOne(r => r.Reviewer)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ReviewsReceived)
            .WithOne(r => r.Reviewee)
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ReportsMade)
            .WithOne(r => r.Reporter)
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Note: User -> Conversation reverse navigation is intentionally omitted.
        // Conversation is configured from its side (User1Id/User2Id -> User).
    }
}