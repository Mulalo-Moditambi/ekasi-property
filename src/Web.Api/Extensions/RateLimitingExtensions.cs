using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Web.Api.Infrastructure;

namespace Web.Api.Extensions;

internal static class RateLimitingExtensions
{
    internal static IServiceCollection AddRateLimitingInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            /*
             * A coarse backstop so nothing is ever completely unlimited, including endpoints
             * added later that forget to name a policy. Set well above any per-endpoint
             * policy: it exists to stop runaway clients, not to shape normal traffic. A
             * limit low enough to bite during ordinary browsing would make the specific
             * policies below unreachable and impossible to reason about.
             */
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(httpContext),
                    factory: _ => FixedWindow(configuration, "Global", permitLimit: 600)));

            AddPolicy(options, configuration, RateLimitingPolicies.Authentication, permitLimit: 10);
            AddPolicy(options, configuration, RateLimitingPolicies.Session, permitLimit: 60);
            AddPolicy(options, configuration, RateLimitingPolicies.Read, permitLimit: 300);
            AddPolicy(options, configuration, RateLimitingPolicies.Write, permitLimit: 60);
            AddPolicy(options, configuration, RateLimitingPolicies.PublicWrite, permitLimit: 10);
        });

        return services;
    }

    private static void AddPolicy(
        RateLimiterOptions options,
        IConfiguration configuration,
        string policyName,
        int permitLimit) =>
        options.AddPolicy(policyName, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetPartitionKey(httpContext),
                factory: _ => FixedWindow(configuration, ConfigKeyFor(policyName), permitLimit)));

    private static FixedWindowRateLimiterOptions FixedWindow(
        IConfiguration configuration,
        string configKey,
        int permitLimit) => new()
        {
            PermitLimit = configuration.GetValue<int?>($"RateLimiting:{configKey}:PermitLimit") ?? permitLimit,
            Window = TimeSpan.FromSeconds(
                configuration.GetValue<int?>($"RateLimiting:{configKey}:WindowInSeconds") ?? 60)
        };

    /// <summary>"public-write" reads back as the "PublicWrite" configuration section.</summary>
    private static string ConfigKeyFor(string policyName) =>
        string.Concat(policyName.Split('-').Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

    /// <summary>
    /// Authenticated callers get their own bucket; everyone else shares one per IP. That
    /// means users behind a single NAT share an anonymous budget, which is why the anonymous
    /// limits are the ones set generously.
    /// </summary>
    private static string GetPartitionKey(HttpContext httpContext)
    {
        return httpContext.User.Identity?.Name
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";
    }
}
