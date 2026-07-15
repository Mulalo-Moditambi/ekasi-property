using Domain.Inquiries;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Inquiries;

internal sealed class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name).HasMaxLength(100);

        builder.Property(i => i.Email).HasMaxLength(255);

        builder.Property(i => i.Phone).HasMaxLength(20);

        builder.Property(i => i.Message).HasMaxLength(2000);

        builder.HasIndex(i => i.PropertyId);

        builder.HasOne<Property>().WithMany().HasForeignKey(i => i.PropertyId);
    }
}
