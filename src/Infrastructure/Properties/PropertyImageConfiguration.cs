using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Properties;

internal sealed class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Url).HasMaxLength(500);

        builder.HasIndex(i => new { i.PropertyId, i.SortOrder });

        builder.HasOne<Property>().WithMany().HasForeignKey(i => i.PropertyId);
    }
}
