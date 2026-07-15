using Application.Abstractions.Messaging;
using Application.Properties.GetById;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Public: anyone browsing the marketplace can view a listing.
        app.MapGet("properties/{id:guid}", async (
            Guid id,
            IQueryHandler<GetPropertyByIdQuery, PropertyResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPropertyByIdQuery(id);

            Result<PropertyResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Properties);
    }
}
