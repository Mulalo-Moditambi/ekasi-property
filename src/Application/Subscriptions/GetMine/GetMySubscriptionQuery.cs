using Application.Abstractions.Messaging;

namespace Application.Subscriptions.GetMine;

public sealed record GetMySubscriptionQuery : IQuery<SubscriptionResponse>;
