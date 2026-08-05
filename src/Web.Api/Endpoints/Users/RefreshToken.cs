using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Refresh;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class RefreshToken : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // The refresh token comes from the httpOnly cookie rather than the request body —
        // the client cannot read it, so it cannot send it.
        app.MapPost("users/refresh-token", async (
            ICommandHandler<RefreshTokenCommand, AccessTokensResponse> handler,
            HttpContext httpContext,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            string? token = RefreshTokenCookie.Read(httpContext.Request);

            if (string.IsNullOrEmpty(token))
            {
                return Results.Unauthorized();
            }

            var command = new RefreshTokenCommand(token);

            Result<AccessTokensResponse> result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                // The cookie is stale or forged; clear it so the browser stops resending it.
                RefreshTokenCookie.Delete(httpContext.Response, configuration);

                return CustomResults.Problem(result);
            }

            return Login.IssueTokens(httpContext, configuration, result.Value);
        })
        .WithTags(Tags.Users)
        .RequireAntiforgery()
        .RequireRateLimiting(RateLimitingPolicies.Session);
    }
}
