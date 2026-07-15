using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Properties.GetById;

internal sealed class GetPropertyByIdQueryHandler(
    IApplicationDbContext context,
    HybridCache cache)
    : IQueryHandler<GetPropertyByIdQuery, PropertyResponse>
{
    public async Task<Result<PropertyResponse>> Handle(GetPropertyByIdQuery query, CancellationToken cancellationToken)
    {
        PropertyResponse? property = await cache.GetOrCreateAsync(
            PropertyCacheKeys.ById(query.PropertyId),
            async cancellation => await context.Properties
                .Where(p => p.Id == query.PropertyId)
                .Select(p => new PropertyResponse
                {
                    Id = p.Id,
                    OwnerId = p.OwnerId,
                    Title = p.Title,
                    Description = p.Description,
                    ListingType = p.ListingType,
                    PropertyType = p.PropertyType,
                    Price = p.Price,
                    Street = p.Address.Street,
                    Township = p.Address.Township,
                    City = p.Address.City,
                    Province = p.Address.Province,
                    PostalCode = p.Address.PostalCode,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    HasElectricity = p.HasElectricity,
                    WaterIncluded = p.WaterIncluded,
                    HasOwnEntrance = p.HasOwnEntrance,
                    HasParking = p.HasParking,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Images = context.PropertyImages
                        .Where(i => i.PropertyId == p.Id)
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new PropertyImageResponse { Id = i.Id, Url = i.Url })
                        .ToList()
                })
                .SingleOrDefaultAsync(cancellation),
            cancellationToken: cancellationToken);

        if (property is null)
        {
            return Result.Failure<PropertyResponse>(PropertyErrors.NotFound(query.PropertyId));
        }

        return property;
    }
}
