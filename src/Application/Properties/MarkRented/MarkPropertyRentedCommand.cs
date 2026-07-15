using Application.Abstractions.Messaging;

namespace Application.Properties.MarkRented;

public sealed record MarkPropertyRentedCommand(Guid PropertyId) : ICommand;
