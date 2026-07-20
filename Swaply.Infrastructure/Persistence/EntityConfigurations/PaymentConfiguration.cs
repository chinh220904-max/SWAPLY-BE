using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
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

        builder.Property(p => p.ProviderTransactionId)
            .HasMaxLength(200);

        builder.Property(p => p.OrderInfo)
            .HasMaxLength(255);

        builder.Property(p => p.PayUrl)
            .HasMaxLength(1000);

        builder.Property(p => p.IpAddress)
            .HasMaxLength(45);

        builder.Property(p => p.ReturnQuery)
            .HasMaxLength(2000);

        builder.Property(p => p.IpnQuery)
            .HasMaxLength(2000);

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

        builder.Property(p => p.PaidAt)
            .IsRequired(false);

        builder.Property(p => p.ExpiresAt)
            .IsRequired(false);

        // 1:1 relationship with Subscription
        builder.HasOne(p => p.Subscription)
            .WithOne(s => s.Payment)
            .HasForeignKey<Swaply.Domain.Entities.Payment>(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
