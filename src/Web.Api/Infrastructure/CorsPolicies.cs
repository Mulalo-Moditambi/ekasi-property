namespace Web.Api.Infrastructure;

internal static class CorsPolicies
{
    /// <summary>
    /// Applied to the /api group. Static assets are same-origin and opt out by not asking
    /// for it — there is no default policy, so nothing gets CORS headers by accident.
    /// </summary>
    internal const string WebClient = "web-client";
}
