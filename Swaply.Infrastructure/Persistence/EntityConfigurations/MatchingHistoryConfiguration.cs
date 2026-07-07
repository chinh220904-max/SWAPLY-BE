using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class MatchingHistoryConfiguration : IEntityTypeConfiguration<MatchingHistory>
{
    public void Configure(EntityTypeBuilder<MatchingHistory> builder)
    {
        builder.ToTable("MatchingHistories");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.SimilarityScore)
            .IsRequired()
            .HasColumnType("decimal(5,4)");

        builder.Property(m => m.IsAccepted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        // Index for finding matches for a listing
        builder.HasIndex(m => m.SourceListingId);

        // Index for finding accepted matches
        builder.HasIndex(m => new { m.SourceListingId, m.IsAccepted });
    }
}
