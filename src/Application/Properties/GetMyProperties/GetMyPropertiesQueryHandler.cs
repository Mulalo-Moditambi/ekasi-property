using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Properties.GetMyProperties;

internal sealed class GetMyPropertiesQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetMyPropertiesQuery, GetMyPropertiesResponse>
{
    private const int MaxPageSize = 50;

    public async Task<Result<GetMyPropertiesResponse>> Handle(
        GetMyPropertiesQuery query,
        CancellationToken cancellationToken)
    {
        int page = Math.Max(query.Page, 1);
        int pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        IQueryable<Property> properties = context.Properties
            .Where(p => p.OwnerId == userContext.UserId);

        if (query.Status.HasValue)
        {
            properties = properties.Where(p => p.Status == query.Status.Value);
        }

        int totalCount = await properties.CountAsync(cancellationToken);

        IOrderedQueryable<Property> ordered = query.Sort switch
        {
            "price_asc" => properties.OrderBy(p => p.Price).ThenByDescending(p => p.CreatedAt),
            "price_desc" => properties.OrderByDescending(p => p.Price).ThenByDescending(p => p.CreatedAt),
            "status" => properties.OrderBy(p => p.Status).ThenByDescending(p => p.CreatedAt),
            _ => properties.OrderByDescending(p => p.CreatedAt),
        };

        List<MyPropertyResponse> items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new MyPropertyResponse
            {
                Id = p.Id,
                Title = p.Title,
                ListingType = p.ListingType,
                PropertyType = p.PropertyType,
                Status = p.Status,
                Price = p.Price,
                Township = p.Address.Township,
                City = p.Address.City,
                Province = p.Address.Province,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CoverImageUrl = context.PropertyImages
                    .Where(i => i.PropertyId == p.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.Url)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new GetMyPropertiesResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            HasNextPage = page * pageSize < totalCount
        };
    }
}
