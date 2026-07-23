namespace Domain.Subscriptions;

public enum SubscriptionStatus
{
    Trialing = 0,
    Active = 1,
    GracePeriod = 2,
    Withdrawn = 3,
    Cancelled = 4
}
