using Application.Abstractions.Messaging;
using Application.Subscriptions.Cancel;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Subscriptions;

internal sealed class Cancel : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("subscriptions/cancel", async (
            ICommandHandler<CancelSubscriptionCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new CancelSubscriptionCommand(), cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Subscriptions)
        .RequireAuthorization();
    }
}
