using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Storage;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.Authentication;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Storage;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddFileStorage(configuration)
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

#pragma warning disable EXTEXP0018 // HybridCache is released; the API is stable in .NET 10.
        services.AddHybridCache();
#pragma warning restore EXTEXP0018

        return services;
    }

    /// <summary>
    /// Blob storage, in every environment. There is no local-disk fallback on purpose: a
    /// second implementation that only ever runs on developer machines is a code path nobody
    /// tests and production never exercises. Locally this points at Azurite
    /// (UseDevelopmentStorage=true), which speaks the same API, so the storage code that runs
    /// on a laptop is the storage code that runs in Azure.
    /// </summary>
    private static IServiceCollection AddFileStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration["AzureBlobStorage:ConnectionString"];
        string? serviceUri = configuration["AzureBlobStorage:ServiceUri"];

        if (string.IsNullOrWhiteSpace(connectionString) && string.IsNullOrWhiteSpace(serviceUri))
        {
            throw new InvalidOperationException(
                "Blob storage is not configured. Set AzureBlobStorage:ConnectionString " +
                "(use 'UseDevelopmentStorage=true' to run against Azurite locally) or " +
                "AzureBlobStorage:ServiceUri for managed-identity access.");
        }

        string containerName = configuration["AzureBlobStorage:ContainerName"] ?? "uploads";

        services.AddSingleton(_ =>
        {
            BlobServiceClient client = string.IsNullOrWhiteSpace(connectionString)
                // Managed identity: no secret to store, rotate or leak into config.
                ? new BlobServiceClient(new Uri(serviceUri!), new DefaultAzureCredential())
                : new BlobServiceClient(connectionString);

            return client.GetBlobContainerClient(containerName);
        });

        // Creating the container is a startup concern, not a request-path one — see
        // BlobContainerInitializer for why that distinction matters.
        services.AddHostedService<BlobContainerInitializer>();

        services.AddSingleton(BlobStorageContentTypes.FromConfiguration(configuration));

        services.AddSingleton<IFileStorage, AzureBlobFileStorage>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(
            options => options
                .UseSqlServer(connectionString, sqlServerOptions =>
                    sqlServerOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Database")!);

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                // Defaults to true so a missing setting fails closed; only the
                // Development config opts out for plain-HTTP local runs.
                o.RequireHttpsMetadata = configuration.GetValue<bool?>("Jwt:RequireHttpsMetadata") ?? true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    // Stated explicitly: these default to true, but leaving them
                    // implicit means a later edit could disable one unnoticed.
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        return services;
    }
}
