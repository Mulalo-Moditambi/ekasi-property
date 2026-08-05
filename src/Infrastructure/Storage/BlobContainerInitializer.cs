using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Storage;

/// <summary>
/// Ensures the uploads container exists before the app serves traffic.
///
/// Creating it lazily inside the DI factory looked equivalent but was not: the factory runs
/// on first resolution, which is the first image upload, so a misconfigured account or an
/// unreachable endpoint surfaced as a 500 for whichever user happened to upload first.
/// Doing it here turns the same failure into a refused startup, which a deployment health
/// check catches before any traffic arrives.
/// </summary>
internal sealed class BlobContainerInitializer(BlobContainerClient container) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Listing photos are served straight to browsers, so the container needs anonymous
        // blob-level read. This throws when the storage account has allowBlobPublicAccess
        // disabled — deliberately fatal, since the alternative is every image 404ing.
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
