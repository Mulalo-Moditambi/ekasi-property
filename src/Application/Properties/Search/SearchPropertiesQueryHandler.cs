using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Properties.Search;

internal sealed class SearchPropertiesQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<SearchPropertiesQuery, SearchPropertiesResponse>
{
    private const int MaxPageSize = 50;

    public async Task<Result<SearchPropertiesResponse>> Handle(
        SearchPropertiesQuery query,
        CancellationToken cancellationToken)
    {
        int page = Math.Max(query.Page, 1);
        int pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        IQueryable<Property> properties = context.Properties
            .Where(p => p.Status == PropertyStatus.Listed);

        if (!string.IsNullOrWhiteSpace(query.Township))
        {
            properties = properties.Where(p => p.Address.Township.Contains(query.Township));
        }

        if (query.ListingType.HasValue)
        {
            properties = properties.Where(p => p.ListingType == query.ListingType.Value);
        }

        if (query.PropertyType.HasValue)
        {
            properties = properties.Where(p => p.PropertyType == query.PropertyType.Value);
        }

        if (query.MinPrice.HasValue)
        {
            properties = properties.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            properties = properties.Where(p => p.Price <= query.MaxPrice.Value);
        }

        if (query.MinBedrooms.HasValue)
        {
            properties = properties.Where(p => p.Bedrooms >= query.MinBedrooms.Value);
        }

        if (query.HasElectricity.HasValue)
        {
            properties = properties.Where(p => p.HasElectricity == query.HasElectricity.Value);
        }

        if (query.WaterIncluded.HasValue)
        {
            properties = properties.Where(p => p.WaterIncluded == query.WaterIncluded.Value);
        }

        if (query.HasOwnEntrance.HasValue)
        {
            properties = properties.Where(p => p.HasOwnEntrance == query.HasOwnEntrance.Value);
        }

        if (query.HasParking.HasValue)
        {
            properties = properties.Where(p => p.HasParking == query.HasParking.Value);
        }

        int totalCount = await properties.CountAsync(cancellationToken);

        IOrderedQueryable<Property> ordered = query.Sort switch
        {
            "price_asc" => properties.OrderBy(p => p.Price).ThenByDescending(p => p.CreatedAt),
            "price_desc" => properties.OrderByDescending(p => p.Price).ThenByDescending(p => p.CreatedAt),
            _ => properties.OrderByDescending(p => p.CreatedAt),
        };

        List<PropertySummaryResponse> items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PropertySummaryResponse
            {
                Id = p.Id,
                Title = p.Title,
                ListingType = p.ListingType,
                PropertyType = p.PropertyType,
                Price = p.Price,
                Township = p.Address.Township,
                City = p.Address.City,
                Province = p.Address.Province,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                HasElectricity = p.HasElectricity,
                WaterIncluded = p.WaterIncluded,
                HasOwnEntrance = p.HasOwnEntrance,
                HasParking = p.HasParking,
                CreatedAt = p.CreatedAt,
                ImageUrls = context.PropertyImages
                    .Where(i => i.PropertyId == p.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.Url)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new SearchPropertiesResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            HasNextPage = page * pageSize < totalCount
        };
    }
}
