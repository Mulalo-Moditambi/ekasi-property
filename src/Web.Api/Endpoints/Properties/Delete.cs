using Application.Abstractions.Messaging;
using Application.Properties.Delete;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("properties/{id:guid}", async (
            Guid id,
            ICommandHandler<DeletePropertyCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeletePropertyCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Write)
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
