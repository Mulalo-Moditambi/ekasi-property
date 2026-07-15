using Domain.Properties;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Properties;

internal sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).HasMaxLength(200);

        builder.Property(p => p.Description).HasMaxLength(4000);

        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.OwnsOne(p => p.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasMaxLength(200);
            addressBuilder.Property(a => a.Township).HasMaxLength(100);
            addressBuilder.Property(a => a.City).HasMaxLength(100);
            addressBuilder.Property(a => a.Province).HasMaxLength(100);
            addressBuilder.Property(a => a.PostalCode).HasMaxLength(10);

            addressBuilder.HasIndex(a => a.Township);
        });

        builder.HasIndex(p => new { p.Status, p.ListingType, p.PropertyType });

        builder.HasOne<User>().WithMany().HasForeignKey(p => p.OwnerId);
    }
}
