using Application.Abstractions.Messaging;
using Application.Properties.MarkRented;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class MarkRented : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("properties/{id:guid}/mark-rented", async (
            Guid id,
            ICommandHandler<MarkPropertyRentedCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new MarkPropertyRentedCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Write)
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
