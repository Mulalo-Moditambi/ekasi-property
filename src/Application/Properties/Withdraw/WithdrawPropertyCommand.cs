using Application.Abstractions.Messaging;

namespace Application.Properties.Withdraw;

public sealed record WithdrawPropertyCommand(Guid PropertyId) : ICommand;
