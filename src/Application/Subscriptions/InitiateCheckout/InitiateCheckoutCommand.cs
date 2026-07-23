using Application.Abstractions.Messaging;

namespace Application.Subscriptions.InitiateCheckout;

public sealed record InitiateCheckoutCommand : ICommand<CheckoutResponse>;

public sealed record CheckoutResponse(string RedirectUrl);
