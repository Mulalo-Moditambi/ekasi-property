using Microsoft.AspNetCore.Antiforgery;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

/// <summary>
/// Issues the CSRF token pair: the secret half is written to an httpOnly cookie, the
/// request half is returned here for the client to echo in <c>X-CSRF-TOKEN</c>.
/// </summary>
internal sealed class AntiforgeryToken : IEndpoint
{
    public sealed record Response(string Token);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("antiforgery/token", (IAntiforgery antiforgery, HttpContext httpContext) =>
        {
            AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(httpContext);

            // Handing out a token is only useful for the request that follows it, so caching
            // one would hand a stale value to the next visitor on a shared proxy.
            httpContext.Response.Headers.CacheControl = "no-store";

            return Results.Ok(new Response(tokens.RequestToken!));
        })
        .WithTags(Tags.Users)
        .RequireRateLimiting(RateLimitingPolicies.Session);
    }
}
