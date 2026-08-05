using Application.Abstractions.Messaging;

namespace Application.Properties.ReorderImages;

public sealed class ReorderPropertyImagesCommand : ICommand
{
    public Guid PropertyId { get; set; }
    public List<Guid> ImageIds { get; set; } = [];
}
