using Domain.Subscriptions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Subscriptions;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => s.OwnerId).IsUnique();

        builder.Property(s => s.PayFastToken).HasMaxLength(100);

        builder.Property(s => s.PayFastMerchantPaymentId).HasMaxLength(100);

        builder.HasOne<User>().WithMany().HasForeignKey(s => s.OwnerId);
    }
}
