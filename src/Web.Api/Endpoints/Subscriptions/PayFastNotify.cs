using Application.Abstractions.Messaging;
using Application.Subscriptions.HandlePayFastNotification;
using Microsoft.AspNetCore.Http;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Subscriptions;

internal sealed class PayFastNotify : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("subscriptions/payfast/itn", async (
            HttpRequest request,
            ICommandHandler<HandlePayFastNotificationCommand> handler,
            CancellationToken cancellationToken) =>
        {
            IFormCollection form = await request.ReadFormAsync(cancellationToken);

            var fields = new Dictionary<string, string>();
            string signature = string.Empty;

            foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> field in form)
            {
                if (field.Key == "signature")
                {
                    signature = field.Value.ToString();
                    continue;
                }

                fields[field.Key] = field.Value.ToString();
            }

            var command = new HandlePayFastNotificationCommand(fields, signature);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(() => Results.Ok(), _ => Results.BadRequest());
        })
        .WithTags(Tags.Subscriptions)
        .AllowAnonymous();
    }
}
