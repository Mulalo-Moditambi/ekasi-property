using Application.Abstractions.Messaging;
using Application.Subscriptions.GetMine;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Subscriptions;

internal sealed class GetMine : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("subscriptions/me", async (
            IQueryHandler<GetMySubscriptionQuery, SubscriptionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<SubscriptionResponse> result = await handler.Handle(new GetMySubscriptionQuery(), cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Subscriptions)
        .RequireAuthorization();
    }
}
