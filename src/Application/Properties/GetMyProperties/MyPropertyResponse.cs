using Domain.Properties;

namespace Application.Properties.GetMyProperties;

public sealed class MyPropertyResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public PropertyStatus Status { get; set; }
    public decimal Price { get; set; }
    public string Township { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CoverImageUrl { get; set; }
}
