namespace Application.Abstractions.Storage;

public interface IFileStorage
{
    /// <summary>
    /// Persists the content and returns the public URL it will be served from.
    /// </summary>
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default);
}
