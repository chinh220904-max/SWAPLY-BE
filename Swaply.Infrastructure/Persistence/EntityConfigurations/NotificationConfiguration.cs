using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.RelatedEntityType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(NotificationEntityType.Unspecified);

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        // Index for user notification queries
        builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

        // Note: User relationship is configured via UserConfiguration (reverse navigation)
    }
}
