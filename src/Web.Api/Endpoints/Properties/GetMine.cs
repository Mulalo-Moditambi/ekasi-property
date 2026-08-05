using Application.Abstractions.Messaging;
using Application.Properties.GetMyProperties;
using Domain.Properties;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class GetMine : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("properties/mine", async (
            IQueryHandler<GetMyPropertiesQuery, GetMyPropertiesResponse> handler,
            CancellationToken cancellationToken,
            int? status,
            string? sort,
            int page = 1,
            int pageSize = 20) =>
        {
            var query = new GetMyPropertiesQuery(
                status.HasValue ? (PropertyStatus)status.Value : null,
                page,
                pageSize,
                sort);

            Result<GetMyPropertiesResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Read)
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
