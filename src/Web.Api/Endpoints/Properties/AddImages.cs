using Application.Abstractions.Messaging;
using Application.Properties.AddImages;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class AddImages : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("properties/{id:guid}/images", async (
            Guid id,
            IFormFileCollection files,
            ICommandHandler<AddPropertyImagesCommand, List<Guid>> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AddPropertyImagesCommand
            {
                PropertyId = id,
                Images = [.. files.Select(file => new ImageUpload(
                    file.OpenReadStream(),
                    file.FileName,
                    file.ContentType,
                    file.Length))]
            };

            Result<List<Guid>> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Properties)
        .RequireAuthorization()
        .DisableAntiforgery();
    }
}
