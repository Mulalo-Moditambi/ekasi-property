using Application.Abstractions.Messaging;
using Application.Inquiries.GetForProperty;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class GetInquiries : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("properties/{id:guid}/inquiries", async (
            Guid id,
            IQueryHandler<GetPropertyInquiriesQuery, List<InquiryResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPropertyInquiriesQuery(id);

            Result<List<InquiryResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Read)
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
