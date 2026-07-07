using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;
using Swaply.Domain.ValueObjects;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Swaply.Domain.Entities.Payment>
{
    public void Configure(EntityTypeBuilder<Swaply.Domain.Entities.Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        // Money as owned value object
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("AmountCurrency")
                .HasMaxLength(10)
                .HasDefaultValue("VND");
        });

        builder.Property(p => p.TransactionId)
            .HasMaxLength(200);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(p => p.Method)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // 1:1 relationship with Subscription
        builder.HasOne(p => p.Subscription)
            .WithOne(s => s.Payment)
            .HasForeignKey<Swaply.Domain.Entities.Payment>(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
