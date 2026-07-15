using SharedKernel;

namespace Domain.Properties;

public sealed class Property : Entity
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public decimal Price { get; set; }
    public Address Address { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public bool HasElectricity { get; set; }
    public bool WaterIncluded { get; set; }
    public bool HasOwnEntrance { get; set; }
    public bool HasParking { get; set; }
    public PropertyStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static Property Create(
        Guid ownerId,
        string title,
        string description,
        ListingType listingType,
        PropertyType propertyType,
        decimal price,
        Address address,
        int bedrooms,
        int bathrooms,
        bool hasElectricity,
        bool waterIncluded,
        bool hasOwnEntrance,
        bool hasParking,
        DateTime utcNow)
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = title,
            Description = description,
            ListingType = listingType,
            PropertyType = propertyType,
            Price = price,
            Address = address,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            HasElectricity = hasElectricity,
            WaterIncluded = waterIncluded,
            HasOwnEntrance = hasOwnEntrance,
            HasParking = hasParking,
            Status = PropertyStatus.Listed,
            CreatedAt = utcNow
        };

        property.Raise(new PropertyListedDomainEvent(property.Id));

        return property;
    }

    public Result MarkAsRented(DateTime utcNow)
    {
        if (ListingType != ListingType.Rent)
        {
            return Result.Failure(PropertyErrors.NotARentalListing(Id));
        }

        if (Status != PropertyStatus.Listed)
        {
            return Result.Failure(PropertyErrors.NotListed(Id));
        }

        Status = PropertyStatus.Rented;
        UpdatedAt = utcNow;

        Raise(new PropertyRentedDomainEvent(Id));

        return Result.Success();
    }

    public Result MarkAsSold(DateTime utcNow)
    {
        if (ListingType != ListingType.Sale)
        {
            return Result.Failure(PropertyErrors.NotASaleListing(Id));
        }

        if (Status != PropertyStatus.Listed)
        {
            return Result.Failure(PropertyErrors.NotListed(Id));
        }

        Status = PropertyStatus.Sold;
        UpdatedAt = utcNow;

        Raise(new PropertySoldDomainEvent(Id));

        return Result.Success();
    }

    public Result Withdraw(DateTime utcNow)
    {
        if (Status != PropertyStatus.Listed)
        {
            return Result.Failure(PropertyErrors.NotListed(Id));
        }

        Status = PropertyStatus.Withdrawn;
        UpdatedAt = utcNow;

        Raise(new PropertyWithdrawnDomainEvent(Id));

        return Result.Success();
    }

    public Result Relist(DateTime utcNow)
    {
        if (Status == PropertyStatus.Sold)
        {
            return Result.Failure(PropertyErrors.AlreadySold(Id));
        }

        if (Status == PropertyStatus.Listed)
        {
            return Result.Failure(PropertyErrors.AlreadyListed(Id));
        }

        Status = PropertyStatus.Listed;
        UpdatedAt = utcNow;

        Raise(new PropertyRelistedDomainEvent(Id));

        return Result.Success();
    }
}
