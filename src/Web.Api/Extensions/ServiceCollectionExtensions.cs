using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Web.Api.Infrastructure;

namespace Web.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Allows browser requests from the configured web client origins only.
    /// In development the Vite dev server proxies /api, so requests are
    /// same-origin and no origins need to be configured; in production
    /// "Cors:Origins" must list the exact scheme+host+port of the client.
    /// </summary>
    internal static IServiceCollection AddCorsInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string[] origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddPolicy(CorsPolicies.WebClient, policy =>
        {
            if (origins.Length == 0)
            {
                // No origins configured: deny every cross-origin request rather
                // than silently falling back to a permissive policy.
                return;
            }

            policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }));

        return services;
    }

    /// <summary>
    /// CSRF protection for the cookie-authenticated endpoints (refresh and logout). Every
    /// other endpoint authenticates with a Bearer header, which a cross-site page cannot
    /// set, so those are not forgeable in the first place.
    ///
    /// The secret half lives in an httpOnly cookie; the client reads its half from the token
    /// endpoint and echoes it in <c>X-CSRF-TOKEN</c>. ASP.NET signs the pair with data
    /// protection and binds it to the current user, so a sibling subdomain that can set
    /// cookies still cannot mint a valid pair.
    /// </summary>
    internal static IServiceCollection AddAntiforgeryInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "ekasi.csrf";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookiePolicy.RequireSecure(configuration)
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
        });

        return services;
    }

    internal static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(static o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter your JWT token in this field",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            };

            o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

            o.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document),
                    []
                }
            });
        });

        return services;
    }
}
