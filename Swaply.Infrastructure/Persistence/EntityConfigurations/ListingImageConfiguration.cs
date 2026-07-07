using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class ListingImageConfiguration : IEntityTypeConfiguration<ListingImage>
{
    public void Configure(EntityTypeBuilder<ListingImage> builder)
    {
        builder.ToTable("ListingImages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(i => i.CloudinaryPublicId)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.HasIndex(i => new { i.ListingId, i.DisplayOrder });
    }
}
