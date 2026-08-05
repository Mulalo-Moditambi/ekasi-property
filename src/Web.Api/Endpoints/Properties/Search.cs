using Application.Abstractions.Messaging;
using Application.Properties.Search;
using Domain.Properties;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class Search : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Public: anyone browsing the marketplace can search listings.
        app.MapGet("properties", async (
            IQueryHandler<SearchPropertiesQuery, SearchPropertiesResponse> handler,
            CancellationToken cancellationToken,
            string? township,
            int? listingType,
            int? propertyType,
            decimal? minPrice,
            decimal? maxPrice,
            int? minBedrooms,
            string? sort,
            bool? hasElectricity,
            bool? waterIncluded,
            bool? hasOwnEntrance,
            bool? hasParking,
            int page = 1,
            int pageSize = 20) =>
        {
            var query = new SearchPropertiesQuery(
                township,
                listingType.HasValue ? (ListingType)listingType.Value : null,
                propertyType.HasValue ? (PropertyType)propertyType.Value : null,
                minPrice,
                maxPrice,
                minBedrooms,
                page,
                pageSize,
                sort,
                hasElectricity,
                waterIncluded,
                hasOwnEntrance,
                hasParking);

            Result<SearchPropertiesResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Read)
        .WithTags(Tags.Properties);
    }
}
