using Application.Abstractions.Messaging;

namespace Application.Properties.Relist;

public sealed record RelistPropertyCommand(Guid PropertyId) : ICommand;
