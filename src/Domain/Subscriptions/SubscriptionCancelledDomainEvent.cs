using SharedKernel;

namespace Domain.Subscriptions;

public sealed record SubscriptionCancelledDomainEvent(Guid SubscriptionId, Guid OwnerId) : IDomainEvent;
