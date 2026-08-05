namespace Web.Api.Infrastructure;

/// <summary>
/// One switch for every cookie the app sets. Two independent flags would eventually
/// disagree, and a cookie that is Secure in one place and not the other is exactly the
/// kind of gap that only shows up in production.
/// </summary>
internal static class CookiePolicy
{
    /// <summary>
    /// Defaults to true so a missing setting fails closed; only the Development and test
    /// configurations opt out, for plain-HTTP local runs.
    /// </summary>
    internal static bool RequireSecure(IConfiguration configuration) =>
        configuration.GetValue<bool?>("Cookies:Secure") ?? true;
}
