using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swaply.Domain.Entities;

namespace Swaply.Infrastructure.Persistence.EntityConfigurations;

public class UserMonthlyQuotaConfiguration : IEntityTypeConfiguration<UserMonthlyQuota>
{
    public void Configure(EntityTypeBuilder<UserMonthlyQuota> builder)
    {
        builder.ToTable("UserMonthlyQuotas");

        builder.HasKey(uq => uq.Id);

        builder.Property(uq => uq.TotalQuota)
            .HasDefaultValue(3);

        builder.Property(uq => uq.UsedQuota)
            .HasDefaultValue(0);

        builder.HasOne(uq => uq.User)
            .WithMany()
            .HasForeignKey(uq => uq.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(uq => new { uq.UserId, uq.Year, uq.Month })
            .IsUnique();
    }
}
