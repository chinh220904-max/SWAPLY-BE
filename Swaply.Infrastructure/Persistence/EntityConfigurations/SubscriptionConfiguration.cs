using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(SubscriptionStatus.Active);

        builder.Property(s => s.StartedAt)
            .IsRequired();

        builder.Property(s => s.ExpiresAt)
            .IsRequired();

        // Index for checking active subscriptions
        builder.HasIndex(s => new { s.UserId, s.Status, s.ExpiresAt });

        builder.HasOne(s => s.Package)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Note: 1:1 with Payment is configured in PaymentConfiguration
        // (Payment is the dependent end with SubscriptionId as FK)
    }
}
