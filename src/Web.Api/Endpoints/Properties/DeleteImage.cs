using Application.Abstractions.Messaging;
using Application.Properties.DeleteImage;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class DeleteImage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("properties/{id:guid}/images/{imageId:guid}", async (
            Guid id,
            Guid imageId,
            ICommandHandler<DeletePropertyImageCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeletePropertyImageCommand(id, imageId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Write)
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
