using Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Azurite;
using Testcontainers.MsSql;
using Web.Api;

namespace IntegrationTests;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    // The app has a single file-storage implementation, so image uploads run against a real
    // blob endpoint here rather than a substitute that could drift from it.
    //
    // --skipApiVersionCheck is required, not optional: the Azure SDK negotiates a REST API
    // version newer than any released Azurite recognises, and without the flag the emulator
    // rejects the very first call with 400 InvalidHeaderValue. Azurite is simply behind the
    // SDK; the app is not doing anything unusual. Appended to the builder's own arguments
    // rather than replacing them.
    private readonly AzuriteContainer _blobContainer =
        new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:latest")
            .WithCommand("--skipApiVersionCheck")
            .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Database", _dbContainer.GetConnectionString());
        builder.UseSetting("AzureBlobStorage:ConnectionString", _blobContainer.GetConnectionString());

        // Provide deterministic JWT settings so tokens can be issued and validated in tests.
        builder.UseSetting("Jwt:Secret", "super-duper-secret-value-that-should-be-in-user-secrets");
        builder.UseSetting("Jwt:Issuer", "clean-architecture");
        builder.UseSetting("Jwt:Audience", "developers");
        builder.UseSetting("Jwt:ExpirationInMinutes", "60");

        // Relax rate limiting so the test suite is not throttled.
        builder.UseSetting("RateLimiting:Global:PermitLimit", "100000");
        builder.UseSetting("RateLimiting:Authentication:PermitLimit", "100000");
        builder.UseSetting("RateLimiting:Session:PermitLimit", "100000");

        // The test server speaks plain HTTP, and a Secure cookie would be stored but never
        // sent back — every refresh would silently fail to find one.
        builder.UseSetting("Cookies:Secure", "false");
    }

    public async Task InitializeAsync()
    {
        // Both must be up before the host is built below: ConfigureWebHost reads their
        // connection strings, and blob storage is resolved during startup.
        await Task.WhenAll(_dbContainer.StartAsync(), _blobContainer.StartAsync());

        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _blobContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
