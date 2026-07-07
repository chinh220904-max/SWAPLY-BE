using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // Unique constraint: a user can only favorite a listing once
        builder.HasIndex(f => new { f.UserId, f.ListingId })
            .IsUnique()
            .HasDatabaseName("IX_Favorites_User_Listing");
    }
}
