using Domain.Subscriptions;

namespace Application.Subscriptions.GetMine;

public sealed class SubscriptionResponse
{
    public Guid Id { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime TrialEndsAt { get; set; }
    public DateTime? GracePeriodEndsAt { get; set; }
    public DateTime? NextBillingDate { get; set; }
}
