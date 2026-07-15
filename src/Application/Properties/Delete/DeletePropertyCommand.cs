using Application.Abstractions.Messaging;

namespace Application.Properties.Delete;

public sealed record DeletePropertyCommand(Guid PropertyId) : ICommand;
