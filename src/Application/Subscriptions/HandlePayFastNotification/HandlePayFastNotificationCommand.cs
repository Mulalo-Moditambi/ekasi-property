using Application.Abstractions.Messaging;

namespace Application.Subscriptions.HandlePayFastNotification;

public sealed record HandlePayFastNotificationCommand(
    IReadOnlyDictionary<string, string> Fields,
    string Signature)
    : ICommand;
