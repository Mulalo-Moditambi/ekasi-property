using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Login;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Login : IEndpoint
{
    public sealed record Request(string Email, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/login", async (
            Request request,
            ICommandHandler<LoginUserCommand, AccessTokensResponse> handler,
            HttpContext httpContext,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);

            Result<AccessTokensResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                tokens => IssueTokens(httpContext, configuration, tokens),
                CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireRateLimiting(RateLimitingPolicies.Authentication);
    }

    /// <summary>
    /// Moves the refresh token into an httpOnly cookie and returns only the access token,
    /// so the long-lived credential is never readable from script.
    /// </summary>
    internal static IResult IssueTokens(
        HttpContext httpContext,
        IConfiguration configuration,
        AccessTokensResponse tokens)
    {
        RefreshTokenCookie.Append(
            httpContext.Response,
            configuration,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresOnUtc);

        return Results.Ok(new AccessTokenResponse(tokens.AccessToken));
    }

    internal sealed record AccessTokenResponse(string AccessToken);
}
