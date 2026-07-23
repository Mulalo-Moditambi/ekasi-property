using Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Web.Api;

namespace IntegrationTests;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Database", _dbContainer.GetConnectionString());

        // Provide deterministic JWT settings so tokens can be issued and validated in tests.
        builder.UseSetting("Jwt:Secret", "super-duper-secret-value-that-should-be-in-user-secrets");
        builder.UseSetting("Jwt:Issuer", "clean-architecture");
        builder.UseSetting("Jwt:Audience", "developers");
        builder.UseSetting("Jwt:ExpirationInMinutes", "60");

        // Relax rate limiting so the test suite is not throttled.
        builder.UseSetting("RateLimiting:Global:PermitLimit", "100000");
        builder.UseSetting("RateLimiting:Authentication:PermitLimit", "100000");

        // Deterministic PayFast settings for signature generation/validation in tests.
        // ValidateUrl points at an unreachable host so the remote ITN re-validation call fails fast
        // and falls back to the already-passed local signature check, instead of hitting the real sandbox.
        builder.UseSetting("PayFast:MerchantId", "10000100");
        builder.UseSetting("PayFast:MerchantKey", "46f0cd694581a");
        builder.UseSetting("PayFast:Passphrase", "test-passphrase");
        builder.UseSetting("PayFast:ValidateUrl", "http://127.0.0.1:1/invalid");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
