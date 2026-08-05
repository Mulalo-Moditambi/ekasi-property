namespace Web.Api.Infrastructure;

/// <summary>
/// Every endpoint names one of these. A global limiter still backstops the whole app, but it
/// is deliberately coarse — a single bucket cannot tell a burst of listing views apart from
/// a password-guessing loop, so the interesting limits live here.
/// </summary>
public static class RateLimitingPolicies
{
    /// <summary>Credential guessing: login and register. Deliberately tight.</summary>
    public const string Authentication = "authentication";

    /// <summary>
    /// Session upkeep: refresh, logout, antiforgery token. Separate from
    /// <see cref="Authentication"/> because these sit on the hot path — every page load
    /// attempts a refresh — and share an IP partition, so a handful of users behind one NAT
    /// would exhaust a brute-force-sized budget and log each other out. There is no password
    /// to guess here: the caller either holds a 256-bit random token or does not.
    /// </summary>
    public const string Session = "session";

    /// <summary>
    /// Reads. Generous: browsing a listing grid fires many of these in a burst, and they are
    /// cheap and side-effect free.
    /// </summary>
    public const string Read = "read";

    /// <summary>
    /// Authenticated mutations. Bounded by what a human editing listings can plausibly do,
    /// so a stolen access token cannot be used to churn data at machine speed.
    /// </summary>
    public const string Write = "write";

    /// <summary>
    /// Anonymous writes — currently just inquiry submission. The tightest non-credential
    /// budget in the app: it is the one endpoint where an unauthenticated caller can create
    /// rows and generate notifications, which makes it the natural spam target.
    /// </summary>
    public const string PublicWrite = "public-write";
}
