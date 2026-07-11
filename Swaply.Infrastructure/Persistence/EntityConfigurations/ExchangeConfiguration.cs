using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class ExchangeConfiguration : IEntityTypeConfiguration<Exchange>
{
    public void Configure(EntityTypeBuilder<Exchange> builder)
    {
        builder.ToTable("Exchanges");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Message)
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ExchangeStatus.Pending);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // ProposerListing relationship
        builder.HasOne(e => e.ProposerListing)
            .WithMany(l => l.ProposedExchanges)
            .HasForeignKey(e => e.ProposerListingId)
            .OnDelete(DeleteBehavior.Restrict);

        // ReceiverListing relationship
        builder.HasOne(e => e.ReceiverListing)
            .WithMany(l => l.ReceivedExchanges)
            .HasForeignKey(e => e.ReceiverListingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Proposer relationship
        builder.HasOne(e => e.Proposer)
            .WithMany(u => u.ProposedExchanges)
            .HasForeignKey(e => e.ProposerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Receiver relationship
        builder.HasOne(e => e.Receiver)
            .WithMany(u => u.ReceivedExchanges)
            .HasForeignKey(e => e.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Reviews)
            .WithOne(r => r.Exchange)
            .HasForeignKey(r => r.ExchangeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
