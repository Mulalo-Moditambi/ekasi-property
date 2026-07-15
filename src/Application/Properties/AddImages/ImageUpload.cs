namespace Application.Properties.AddImages;

public sealed record ImageUpload(Stream Content, string FileName, string ContentType, long Length);
