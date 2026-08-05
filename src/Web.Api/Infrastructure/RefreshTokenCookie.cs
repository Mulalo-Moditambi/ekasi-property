namespace Web.Api.Infrastructure;

/// <summary>
/// The refresh token travels as an httpOnly cookie so injected script cannot read it.
/// Set, read and delete all go through here: the browser only clears a cookie when the
/// delete options match the ones it was written with, so these must never drift apart.
/// </summary>
internal static class RefreshTokenCookie
{
    internal const string Name = "ekasi.refreshToken";

    private static CookieOptions BaseOptions(IConfiguration configuration) => new()
    {
        HttpOnly = true,
        Secure = CookiePolicy.RequireSecure(configuration),
        SameSite = SameSiteMode.Strict,
        // Scoped to "/" deliberately. The Vite dev server proxies /api/* to the API and
        // rewrites the prefix away, so a cookie pathed at the API's own route would never
        // be sent in development. HttpOnly + SameSite=Strict carry the security weight.
        Path = "/"
    };

    internal static void Append(
        HttpResponse response,
        IConfiguration configuration,
        string refreshToken,
        DateTime expiresOnUtc)
    {
        CookieOptions options = BaseOptions(configuration);
        options.Expires = new DateTimeOffset(expiresOnUtc, TimeSpan.Zero);

        response.Cookies.Append(Name, refreshToken, options);
    }

    internal static void Delete(HttpResponse response, IConfiguration configuration) =>
        response.Cookies.Delete(Name, BaseOptions(configuration));

    internal static string? Read(HttpRequest request) =>
        request.Cookies.TryGetValue(Name, out string? token) ? token : null;
}
