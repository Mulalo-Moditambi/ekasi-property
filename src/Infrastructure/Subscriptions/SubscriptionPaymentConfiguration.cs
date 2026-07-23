using Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Subscriptions;

internal sealed class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PfPaymentId).HasMaxLength(100);

        builder.HasIndex(p => p.PfPaymentId).IsUnique();

        builder.Property(p => p.PaymentStatus).HasMaxLength(50);

        builder.Property(p => p.AmountGross).HasPrecision(18, 2);

        builder.HasOne<Subscription>().WithMany().HasForeignKey(p => p.SubscriptionId);
    }
}
