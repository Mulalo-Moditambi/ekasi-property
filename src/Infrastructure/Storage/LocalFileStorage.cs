using Application.Abstractions.Storage;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Storage;

/// <summary>
/// Stores files under wwwroot/uploads so they are served as static files at /uploads/*.
/// Swap for a blob-storage implementation when moving off a single host.
/// </summary>
internal sealed class LocalFileStorage(IHostEnvironment environment) : IFileStorage
{
    private const string RequestPath = "/uploads";

    private readonly string _root = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads");

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_root);

        string fileName = $"{Guid.NewGuid():N}{extension}";
        string fullPath = Path.Combine(_root, fileName);

        await using FileStream fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"{RequestPath}/{fileName}";
    }
}
