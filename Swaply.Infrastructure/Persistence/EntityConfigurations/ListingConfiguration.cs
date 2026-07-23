using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.ValueObjects;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("Listings");

        builder.HasKey(l => l.Id);

        builder.HasQueryFilter(l => !l.IsDeleted);

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .IsRequired()
            .HasMaxLength(5000);

        // Money as owned value object
        builder.OwnsOne(l => l.EstimatedValue, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("EstimatedValue")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            money.Property(m => m.Currency)
                .HasColumnName("EstimatedValueCurrency")
                .HasMaxLength(10)
                .HasDefaultValue("VND");
        });

        builder.OwnsOne(l => l.CashTopUp, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CashTopUpAmount")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("CashTopUpCurrency")
                .HasMaxLength(10)
                .HasDefaultValue("VND");
        });

        builder.Navigation(l => l.CashTopUp).IsRequired(false);

        // Enum conversions
        builder.Property(l => l.Condition)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ListingStatus.Draft);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // No delete cascade on OwnerId — User->Listing is Restrict (handled in UserConfiguration)
        builder.HasMany(l => l.Images)
            .WithOne(i => i.Listing)
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.ProposedExchanges)
            .WithOne(e => e.ProposerListing)
            .HasForeignKey(e => e.ProposerListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.ReceivedExchanges)
            .WithOne(e => e.ReceiverListing)
            .HasForeignKey(e => e.ReceiverListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.SourceMatches)
            .WithOne(m => m.SourceListing)
            .HasForeignKey(m => m.SourceListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.MatchedAsSource)
            .WithOne(m => m.MatchedListing)
            .HasForeignKey(m => m.MatchedListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Favorites)
            .WithOne(f => f.Listing)
            .HasForeignKey(f => f.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
