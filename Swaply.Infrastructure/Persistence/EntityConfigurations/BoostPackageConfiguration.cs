using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class BoostPackageConfiguration : IEntityTypeConfiguration<BoostPackage>
{
    public void Configure(EntityTypeBuilder<BoostPackage> builder)
    {
        builder.ToTable("BoostPackages");

        builder.HasKey(bp => bp.Id);

        builder.Property(bp => bp.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(bp => bp.Description)
            .HasMaxLength(1000);

        builder.Property(bp => bp.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(bp => bp.Priority)
            .HasDefaultValue(100);

        builder.Property(bp => bp.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(bp => bp.Subscriptions)
            .WithOne(bs => bs.BoostPackage)
            .HasForeignKey(bs => bs.BoostPackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
