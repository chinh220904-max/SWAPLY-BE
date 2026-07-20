using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ToTable("Packages");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.DurationDays)
            .IsRequired();

        builder.Property(p => p.MaxListings)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasData(
            new Package(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Basic", "Basic plan with 5 listings limit", 99000m, 30, 5),
            new Package(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Premium", "Premium plan with unlimited listings", 199000m, 30, 999)
        );
    }
}
