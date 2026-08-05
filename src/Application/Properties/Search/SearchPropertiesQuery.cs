using Application.Abstractions.Messaging;
using Domain.Properties;

namespace Application.Properties.Search;

public sealed record SearchPropertiesQuery(
    string? Township,
    ListingType? ListingType,
    PropertyType? PropertyType,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? MinBedrooms,
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    bool? HasElectricity = null,
    bool? WaterIncluded = null,
    bool? HasOwnEntrance = null,
    bool? HasParking = null) : IQuery<SearchPropertiesResponse>;
