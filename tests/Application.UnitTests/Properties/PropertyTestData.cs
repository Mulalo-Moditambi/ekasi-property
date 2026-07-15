using Domain.Properties;

namespace Application.UnitTests.Properties;

internal static class PropertyTestData
{
    internal static Property CreateProperty(
        Guid ownerId,
        ListingType listingType = ListingType.Rent,
        PropertyStatus status = PropertyStatus.Listed)
    {
        return new Property
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = "Neat backroom with own entrance",
            Description = "A tidy backroom in Soweto, prepaid electricity included.",
            ListingType = listingType,
            PropertyType = PropertyType.Backroom,
            Price = 1800m,
            Address = new Address("123 Vilakazi Street", "Orlando West", "Soweto", "Gauteng", "1804"),
            Bedrooms = 1,
            Bathrooms = 1,
            HasElectricity = true,
            WaterIncluded = true,
            HasOwnEntrance = true,
            HasParking = false,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }
}
