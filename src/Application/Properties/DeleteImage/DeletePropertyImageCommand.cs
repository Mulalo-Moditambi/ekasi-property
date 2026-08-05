using Application.Abstractions.Messaging;

namespace Application.Properties.DeleteImage;

public sealed record DeletePropertyImageCommand(Guid PropertyId, Guid ImageId) : ICommand;
