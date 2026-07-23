using SharedKernel;

namespace Domain.Subscriptions;

public sealed record SubscriptionActivatedDomainEvent(Guid SubscriptionId, Guid OwnerId) : IDomainEvent;
