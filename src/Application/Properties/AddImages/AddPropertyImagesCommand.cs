using Application.Abstractions.Messaging;

namespace Application.Properties.AddImages;

public sealed class AddPropertyImagesCommand : ICommand<List<Guid>>
{
    public Guid PropertyId { get; set; }
    public List<ImageUpload> Images { get; set; } = [];
}
