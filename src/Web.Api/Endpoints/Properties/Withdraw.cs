using Application.Abstractions.Messaging;
using Application.Properties.Withdraw;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class Withdraw : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("properties/{id:guid}/withdraw", async (
            Guid id,
            ICommandHandler<WithdrawPropertyCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new WithdrawPropertyCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Write)
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
