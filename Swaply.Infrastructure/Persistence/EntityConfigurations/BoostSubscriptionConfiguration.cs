using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class BoostSubscriptionConfiguration : IEntityTypeConfiguration<BoostSubscription>
{
    public void Configure(EntityTypeBuilder<BoostSubscription> builder)
    {
        builder.ToTable("BoostSubscriptions");

        builder.HasKey(bs => bs.Id);

        builder.Property(bs => bs.TotalQuota)
            .IsRequired();

        builder.Property(bs => bs.UsedQuota)
            .HasDefaultValue(0);

        builder.Property(bs => bs.Status)
            .HasConversion<string>();

        builder.HasOne(bs => bs.User)
            .WithMany()
            .HasForeignKey(bs => bs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bs => new { bs.UserId, bs.Status });
    }
}
