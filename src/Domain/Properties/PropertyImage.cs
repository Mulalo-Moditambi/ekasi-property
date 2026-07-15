using SharedKernel;

namespace Domain.Properties;

public sealed class PropertyImage : Entity
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string Url { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public static PropertyImage Create(Guid propertyId, string url, int sortOrder, DateTime utcNow)
    {
        var image = new PropertyImage
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            Url = url,
            SortOrder = sortOrder,
            CreatedAt = utcNow
        };

        image.Raise(new PropertyImageAddedDomainEvent(image.Id));

        return image;
    }
}
