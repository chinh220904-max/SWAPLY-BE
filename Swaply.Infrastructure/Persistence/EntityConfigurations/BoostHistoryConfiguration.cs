using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class BoostHistoryConfiguration : IEntityTypeConfiguration<BoostHistory>
{
    public void Configure(EntityTypeBuilder<BoostHistory> builder)
    {
        builder.ToTable("BoostHistories");

        builder.HasKey(bh => bh.Id);

        builder.Property(bh => bh.Priority)
            .IsRequired();

        builder.HasOne(bh => bh.Listing)
            .WithMany()
            .HasForeignKey(bh => bh.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bh => bh.User)
            .WithMany()
            .HasForeignKey(bh => bh.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bh => bh.BoostSubscription)
            .WithMany(bs => bs.BoostHistories)
            .HasForeignKey(bh => bh.BoostSubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bh => bh.ListingId);
        builder.HasIndex(bh => bh.ExpiresAt);
    }
}
