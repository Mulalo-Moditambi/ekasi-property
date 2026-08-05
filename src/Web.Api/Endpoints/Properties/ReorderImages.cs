using Application.Abstractions.Messaging;
using Application.Properties.ReorderImages;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class ReorderImages : IEndpoint
{
    public sealed class Request
    {
        public List<Guid> ImageIds { get; set; } = [];
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("properties/{id:guid}/images/order", async (
            Guid id,
            Request request,
            ICommandHandler<ReorderPropertyImagesCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ReorderPropertyImagesCommand
            {
                PropertyId = id,
                ImageIds = request.ImageIds
            };

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireRateLimiting(RateLimitingPolicies.Write)
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
