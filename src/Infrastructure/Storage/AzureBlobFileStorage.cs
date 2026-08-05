using Application.Abstractions.Storage;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure.Storage;

/// <summary>
/// Stores listing images in Azure Blob Storage and returns their absolute public URL.
///
/// This is the only <see cref="IFileStorage"/> implementation, in every environment —
/// locally it runs against Azurite. Writing to the container filesystem instead would lose
/// every upload on the next deploy and hide them from sibling replicas.
///
/// Listing photos are public by design, so the container is created with anonymous blob-level
/// read. That requires the storage account itself to permit anonymous access
/// (allowBlobPublicAccess = true); with it disabled, container creation fails fast at startup
/// rather than silently serving 404s to every visitor.
/// </summary>
internal sealed class AzureBlobFileStorage : IFileStorage
{
    private readonly BlobContainerClient _container;
    private readonly IReadOnlyDictionary<string, string> _contentTypes;

    public AzureBlobFileStorage(BlobContainerClient container, BlobStorageContentTypes contentTypes)
    {
        _container = container;
        _contentTypes = contentTypes.Map;
    }

    public async Task<string> SaveAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default)
    {
        string blobName = $"{Guid.NewGuid():N}{extension}";

        BlobClient blob = _container.GetBlobClient(blobName);

        await blob.UploadAsync(
            content,
            new BlobUploadOptions
            {
                // Without an explicit content type the browser gets application/octet-stream
                // and downloads the file instead of rendering it in an <img>.
                HttpHeaders = new BlobHttpHeaders { ContentType = ContentTypeFor(extension) }
            },
            cancellationToken);

        return blob.Uri.ToString();
    }

    public async Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        string blobName = BlobNameFrom(url);

        if (blobName.Length == 0)
        {
            return;
        }

        try
        {
            await _container.DeleteBlobIfExistsAsync(blobName, cancellationToken: cancellationToken);
        }
        catch (RequestFailedException)
        {
            // Deleting an image that is already gone leaves the caller where it wanted to be.
            // Failing here would block the listing edit that triggered it.
        }
    }

    /// <summary>
    /// Takes the last path segment, so this handles both the absolute blob URLs written here
    /// and the legacy "/uploads/{file}" paths left behind by the old local-disk storage.
    /// </summary>
    private static string BlobNameFrom(string url)
    {
        string path = Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ? uri.AbsolutePath : url;

        return path.Split('/', StringSplitOptions.RemoveEmptyEntries) is { Length: > 0 } segments
            ? segments[^1]
            : string.Empty;
    }

    private string ContentTypeFor(string extension) =>
        _contentTypes.TryGetValue(BlobStorageContentTypes.Normalize(extension), out string? contentType)
            ? contentType
            // Unknown extensions are stored, but as an opaque download rather than something
            // a browser will render — guessing a type for unrecognised bytes is how an
            // upload ends up being served as script.
            : "application/octet-stream";
}
