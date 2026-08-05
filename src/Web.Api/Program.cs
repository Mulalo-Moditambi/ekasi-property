using System.Reflection;
using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Web.Api;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSwaggerGenWithAuth();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddObservability(builder.Configuration, builder.Environment.ApplicationName);

builder.Services.AddRateLimitingInternal(builder.Configuration);

builder.Services.AddCorsInternal(builder.Configuration);

builder.Services.AddAntiforgeryInternal(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

WebApplication app = builder.Build();

/*
 * Pipeline order below is load-bearing; the grouping is deliberate:
 *
 *   1. exception handling and logging wrap everything downstream
 *   2. static files short-circuit before auth and rate limiting, so a page load's worth of
 *      hashed assets never spends the caller's API budget
 *   3. CORS precedes authentication so a preflight is answered before any credential check
 *      can reject it
 *   4. rate limiting sits after authentication so limits partition by user, not just by IP
 *   5. antiforgery runs last, once we know who the caller claims to be
 */

app.UseExceptionHandler();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

// The published SPA. Served ahead of the limiter: a cold page load pulls a dozen hashed
// chunks, and counting those against the API budget would 429 real users mid-navigation.
app.UseStaticFiles();

// Uploaded listing images are not served from here: blob storage (Azurite locally) hands out
// its own absolute URLs and the browser fetches them directly.

// No default policy is configured, so this only acts on endpoints that opt in via
// RequireCors — the /api group below. Static assets are same-origin and need no headers.
app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.UseAntiforgery();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();

    app.ApplyMigrations();
}

// Everything the SPA calls lives under /api, matching the path the browser actually
// requests. Health checks stay outside it so probes never depend on the API surface.
app.MapEndpoints(app.MapGroup("api").RequireCors(CorsPolicies.WebClient));

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
})
// Orchestrator probes hit this on a fixed schedule from a small set of IPs. Counting them
// against a shared bucket would let liveness checks throttle themselves into failure.
.DisableRateLimiting();

// REMARK: If you want to use Controllers, you'll need this.
app.MapControllers();

// An unmatched /api path is a bug, not a deep link — without this the SPA fallback would
// answer it with index.html and a 200, turning a typo into a confusing HTML response.
app.MapFallback("/api/{**segment}", () => Results.NotFound());

// Client-side routes (/manager/properties, /properties/{id}) have no server counterpart;
// hand them the shell and let the router resolve them.
app.MapFallbackToFile("index.html");

await app.RunAsync();

// REMARK: Required for functional and integration tests to work.
namespace Web.Api
{
    public partial class Program;
}
