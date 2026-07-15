using Application.Abstractions.Messaging;
using Application.Properties.MarkSold;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class MarkSold : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("properties/{id:guid}/mark-sold", async (
            Guid id,
            ICommandHandler<MarkPropertySoldCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new MarkPropertySoldCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Properties)
        .RequireAuthorization();
    }
}
