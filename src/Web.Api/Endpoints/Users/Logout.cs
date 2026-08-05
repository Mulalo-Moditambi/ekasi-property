using Application.Abstractions.Messaging;
using Application.Users.Logout;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/logout", async (
            ICommandHandler<LogoutCommand> handler,
            HttpContext httpContext,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            string? token = RefreshTokenCookie.Read(httpContext.Request);

            if (!string.IsNullOrEmpty(token))
            {
                Result result = await handler.Handle(new LogoutCommand(token), cancellationToken);

                if (result.IsFailure)
                {
                    return CustomResults.Problem(result);
                }
            }

            // Cleared unconditionally: a caller with no cookie, or one holding a token the
            // server has already forgotten, should still end up logged out.
            RefreshTokenCookie.Delete(httpContext.Response, configuration);

            return Results.NoContent();
        })
        .WithTags(Tags.Users)
        .RequireAntiforgery()
        .RequireRateLimiting(RateLimitingPolicies.Session);
    }
}
