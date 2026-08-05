using Microsoft.Extensions.Configuration;

namespace Infrastructure.Storage;

/// <summary>
/// Maps a file extension to the Content-Type stored on the blob, so browsers render uploads
/// instead of downloading them.
///
/// Configured under "AzureBlobStorage:ContentTypes" as extension/type pairs. Extensions are
/// written without a leading dot — a dot is legal in a configuration key but awkward to
/// override through an environment variable, where the key becomes
/// AzureBlobStorage__ContentTypes__jpg.
///
/// Keep this in step with the upload allow-list in AddPropertyImagesCommandValidator: a type
/// accepted there but missing here is stored as an opaque download, and a type listed here
/// but rejected there is simply unreachable.
/// </summary>
internal sealed class BlobStorageContentTypes
{
    internal const string SectionName = "AzureBlobStorage:ContentTypes";

    /// <summary>
    /// Used when the section is absent. Mirrors the validator's allow-list, so the default
    /// deployment renders every format it actually accepts.
    /// </summary>
    private static readonly Dictionary<string, string> Defaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["jpg"] = "image/jpeg",
        ["jpeg"] = "image/jpeg",
        ["png"] = "image/png",
        ["webp"] = "image/webp"
    };

    private BlobStorageContentTypes(IReadOnlyDictionary<string, string> map) => Map = map;

    internal IReadOnlyDictionary<string, string> Map { get; }

    internal static BlobStorageContentTypes FromConfiguration(IConfiguration configuration)
    {
        var configured = configuration
            .GetSection(SectionName)
            .GetChildren()
            .Where(child => !string.IsNullOrWhiteSpace(child.Value))
            .ToDictionary(child => Normalize(child.Key), child => child.Value!, StringComparer.OrdinalIgnoreCase);

        // Replaced wholesale rather than merged with the defaults: a deployment that narrows
        // the list expects the removed entries to be gone, not quietly reinstated.
        return new BlobStorageContentTypes(configured.Count > 0 ? configured : Defaults);
    }

    /// <summary>Accepts ".jpg" or "jpg" — callers pass Path.GetExtension output, which keeps the dot.</summary>
    internal static string Normalize(string extension) => extension.TrimStart('.');
}
