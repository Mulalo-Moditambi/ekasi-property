namespace Domain.Properties;

public sealed record Address(
    string Street,
    string Township,
    string City,
    string Province,
    string PostalCode);
