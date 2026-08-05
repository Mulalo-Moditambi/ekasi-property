using Application.Abstractions.Messaging;
using Application.Inquiries.GetMine;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Inquiries;

internal sealed class GetMine : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("inquiries/mine", async (
            IQueryHandler<GetMyLeadsQuery, GetMyLeadsResponse> handler,
            CancellationToken cancellationToken,
            Guid? propertyId,
            string? search,
            int page = 1,
            int pageSize = 20) =>
        {
            var query = new GetMyLeadsQuery(propertyId, search, page, pageSize);

            Result<GetMyLeadsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Read)
        .WithTags(Tags.Inquiries)
        .RequireAuthorization();
    }
}
