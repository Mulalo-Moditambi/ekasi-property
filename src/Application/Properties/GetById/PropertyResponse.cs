using Domain.Properties;

namespace Application.Properties.GetById;

public sealed class PropertyResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public decimal Price { get; set; }
    public string Street { get; set; }
    public string Township { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string PostalCode { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public bool HasElectricity { get; set; }
    public bool WaterIncluded { get; set; }
    public bool HasOwnEntrance { get; set; }
    public bool HasParking { get; set; }
    public PropertyStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PropertyImageResponse> Images { get; set; } = [];
}
