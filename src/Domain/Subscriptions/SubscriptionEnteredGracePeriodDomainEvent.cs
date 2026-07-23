using SharedKernel;

namespace Domain.Subscriptions;

public sealed record SubscriptionEnteredGracePeriodDomainEvent(Guid SubscriptionId, Guid OwnerId, DateTime GracePeriodEndsAt)
    : IDomainEvent;
