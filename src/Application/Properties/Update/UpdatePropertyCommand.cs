using Application.Abstractions.Messaging;

namespace Application.Properties.Update;

public sealed class UpdatePropertyCommand : ICommand
{
    public Guid PropertyId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
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
}
