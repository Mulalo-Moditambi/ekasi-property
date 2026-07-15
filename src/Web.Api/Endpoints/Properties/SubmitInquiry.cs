using Application.Abstractions.Messaging;
using Application.Inquiries.Submit;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Properties;

internal sealed class SubmitInquiry : IEndpoint
{
    public sealed record Request(string Name, string Email, string? Phone, string Message);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Public: browsers can contact the owner without an account.
        app.MapPost("properties/{id:guid}/inquiries", async (
            Guid id,
            Request request,
            ICommandHandler<SubmitInquiryCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new SubmitInquiryCommand
            {
                PropertyId = id,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Message = request.Message
            };

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Properties);
    }
}
