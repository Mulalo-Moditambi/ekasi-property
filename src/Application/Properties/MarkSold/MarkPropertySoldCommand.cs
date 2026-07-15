using Application.Abstractions.Messaging;

namespace Application.Properties.MarkSold;

public sealed record MarkPropertySoldCommand(Guid PropertyId) : ICommand;
