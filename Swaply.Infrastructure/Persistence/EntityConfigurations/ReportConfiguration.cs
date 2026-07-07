using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(r => r.Description)
            .HasMaxLength(2000);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ReportStatus.Pending);

        builder.Property(r => r.AdminNote)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // Index for pending reports queue
        builder.HasIndex(r => new { r.Status, r.CreatedAt });

        // TargetType + TargetId composite for polymorphic relationship
        builder.Property(r => r.TargetType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
