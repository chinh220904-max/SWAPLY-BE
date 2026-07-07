using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Unique conversation between two users: (User1Id, User2Id) must be unique
        // Note: User1Id should always store the smaller Guid (enforced at Application layer)
        builder.HasIndex(c => new { c.User1Id, c.User2Id })
            .IsUnique()
            .HasDatabaseName("IX_Conversations_User1_User2");

        // User1 relationship (conversation participant 1)
        builder.HasOne(c => c.User1)
            .WithMany()
            .HasForeignKey(c => c.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        // User2 relationship (conversation participant 2)
        builder.HasOne(c => c.User2)
            .WithMany()
            .HasForeignKey(c => c.User2Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Optional relationship to a Listing
        builder.HasOne(c => c.RelatedListing)
            .WithMany()
            .HasForeignKey(c => c.RelatedListingId)
            .OnDelete(DeleteBehavior.SetNull);

        // Optional relationship to an Exchange
        builder.HasOne(c => c.RelatedExchange)
            .WithMany()
            .HasForeignKey(c => c.RelatedExchangeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
