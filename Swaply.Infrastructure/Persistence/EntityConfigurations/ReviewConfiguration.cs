using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(2000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // Unique constraint: each participant can only review an exchange once
        builder.HasIndex(r => new { r.ExchangeId, r.ReviewerId })
            .IsUnique()
            .HasDatabaseName("IX_Reviews_Exchange_Reviewer");
    }
}
