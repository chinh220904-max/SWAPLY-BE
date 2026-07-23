using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class BoostPackageGoldenHourConfiguration : IEntityTypeConfiguration<BoostPackageGoldenHour>
{
    public void Configure(EntityTypeBuilder<BoostPackageGoldenHour> builder)
    {
        builder.ToTable("BoostPackageGoldenHours");

        builder.HasKey(bg => bg.Id);

        builder.Property(bg => bg.StartTime)
            .IsRequired();

        builder.Property(bg => bg.EndTime)
            .IsRequired();

        builder.HasOne(bg => bg.BoostPackage)
            .WithMany()
            .HasForeignKey(bg => bg.BoostPackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
