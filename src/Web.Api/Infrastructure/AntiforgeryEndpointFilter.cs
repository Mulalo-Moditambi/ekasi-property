using Microsoft.AspNetCore.Antiforgery;

namespace Web.Api.Infrastructure;

/// <summary>
/// Validates the CSRF token on endpoints that authenticate from a cookie.
///
/// <c>UseAntiforgery</c> only auto-validates requests that carry form data, so JSON and
/// bodiless POSTs would otherwise pass straight through. Applied explicitly rather than
/// globally: Bearer-authenticated endpoints gain nothing from it, and requiring a token
/// there would make every API call depend on the token round trip.
/// </summary>
internal sealed class AntiforgeryEndpointFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            // 403 rather than 400: the request was understood, the caller just could not
            // prove it came from the app. The client refetches a token and retries once.
            return Results.Problem(
                title: "Antiforgery.ValidationFailed",
                detail: "The anti-forgery token is missing or invalid.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}
