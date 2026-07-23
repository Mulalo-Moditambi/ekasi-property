using SharedKernel;

namespace Domain.Subscriptions;

public sealed record SubscriptionWithdrawnDomainEvent(Guid SubscriptionId, Guid OwnerId) : IDomainEvent;
