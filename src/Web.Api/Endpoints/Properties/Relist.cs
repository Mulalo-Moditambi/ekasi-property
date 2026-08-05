using Application.Abstractions.Messaging;
using Application.Properties.Relist;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class Relist : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("properties/{id:guid}/relist", async (
            Guid id,
            ICommandHandler<RelistPropertyCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RelistPropertyCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Write)
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
