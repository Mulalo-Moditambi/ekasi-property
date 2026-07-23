using SharedKernel;

namespace Domain.Subscriptions;

public sealed record SubscriptionTrialStartedDomainEvent(Guid SubscriptionId, Guid OwnerId) : IDomainEvent;
