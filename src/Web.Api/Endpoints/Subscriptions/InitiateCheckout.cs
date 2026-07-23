using Application.Abstractions.Messaging;
using Application.Subscriptions.InitiateCheckout;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Subscriptions;

internal sealed class InitiateCheckout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("subscriptions/checkout", async (
            ICommandHandler<InitiateCheckoutCommand, CheckoutResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<CheckoutResponse> result = await handler.Handle(new InitiateCheckoutCommand(), cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Subscriptions)
        .RequireAuthorization();
    }
}
