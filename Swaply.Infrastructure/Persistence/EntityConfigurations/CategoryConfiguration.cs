using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(2048);

        builder.HasMany(c => c.Listings)
            .WithOne(l => l.Category)
            .HasForeignKey(l => l.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Category(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Baby Gear", "Cribs, strollers, car seats, etc.", "baby-gear.png"),
            new Category(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Toys", "Action figures, dolls, board games, etc.", "toys.png")
        );
    }
}
