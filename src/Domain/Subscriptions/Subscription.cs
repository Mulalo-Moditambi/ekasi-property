using SharedKernel;

namespace Domain.Subscriptions;

public sealed class Subscription : Entity
{
    public const decimal MonthlyFeeZar = 25.00m;

    private const int TrialDurationDays = 14;
    private const int GracePeriodDurationDays = 7;

    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public SubscriptionStatus Status { get; set; }

    public DateTime TrialStartedAt { get; set; }
    public DateTime TrialEndsAt { get; set; }

    public DateTime? GracePeriodStartedAt { get; set; }
    public DateTime? GracePeriodEndsAt { get; set; }
    public DateTime? Day1ReminderSentAt { get; set; }
    public DateTime? Day4ReminderSentAt { get; set; }
    public DateTime? Day6ReminderSentAt { get; set; }

    public DateTime? LastPaymentAt { get; set; }
    public DateTime? NextBillingDate { get; set; }

    public string? PayFastToken { get; set; }
    public string? PayFastMerchantPaymentId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static Subscription StartTrial(Guid ownerId, DateTime utcNow)
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Status = SubscriptionStatus.Trialing,
            TrialStartedAt = utcNow,
            TrialEndsAt = utcNow.AddDays(TrialDurationDays),
            CreatedAt = utcNow
        };

        subscription.Raise(new SubscriptionTrialStartedDomainEvent(subscription.Id, ownerId));

        return subscription;
    }

    public bool GrantsListingAccess() => Status is SubscriptionStatus.Trialing or SubscriptionStatus.Active;

    public Result EnterGracePeriod(DateTime utcNow)
    {
        if (Status is not (SubscriptionStatus.Trialing or SubscriptionStatus.Active))
        {
            return Result.Failure(SubscriptionErrors.InvalidTransition(Id, Status, SubscriptionStatus.GracePeriod));
        }

        Status = SubscriptionStatus.GracePeriod;
        GracePeriodStartedAt = utcNow;
        GracePeriodEndsAt = utcNow.AddDays(GracePeriodDurationDays);
        Day1ReminderSentAt = null;
        Day4ReminderSentAt = null;
        Day6ReminderSentAt = null;
        UpdatedAt = utcNow;

        Raise(new SubscriptionEnteredGracePeriodDomainEvent(Id, OwnerId, GracePeriodEndsAt.Value));

        return Result.Success();
    }

    public Result RecordReminderSent(int dayNumber, DateTime utcNow)
    {
        if (Status != SubscriptionStatus.GracePeriod)
        {
            return Result.Failure(SubscriptionErrors.NotInGracePeriod(Id));
        }

        switch (dayNumber)
        {
            case 1:
                Day1ReminderSentAt = utcNow;
                break;
            case 4:
                Day4ReminderSentAt = utcNow;
                break;
            case 6:
                Day6ReminderSentAt = utcNow;
                break;
            default:
                return Result.Failure(SubscriptionErrors.InvalidReminderDay(dayNumber));
        }

        return Result.Success();
    }

    public Result RecordSuccessfulPayment(DateTime utcNow, DateTime nextBillingDate, string? payFastToken)
    {
        if (Status == SubscriptionStatus.Cancelled)
        {
            return Result.Failure(SubscriptionErrors.Cancelled(Id));
        }

        Status = SubscriptionStatus.Active;
        LastPaymentAt = utcNow;
        NextBillingDate = nextBillingDate;
        GracePeriodStartedAt = null;
        GracePeriodEndsAt = null;
        Day1ReminderSentAt = null;
        Day4ReminderSentAt = null;
        Day6ReminderSentAt = null;

        if (payFastToken is not null)
        {
            PayFastToken = payFastToken;
        }

        UpdatedAt = utcNow;

        Raise(new SubscriptionActivatedDomainEvent(Id, OwnerId));

        return Result.Success();
    }

    public Result MarkWithdrawn(DateTime utcNow)
    {
        if (Status != SubscriptionStatus.GracePeriod)
        {
            return Result.Failure(SubscriptionErrors.InvalidTransition(Id, Status, SubscriptionStatus.Withdrawn));
        }

        Status = SubscriptionStatus.Withdrawn;
        UpdatedAt = utcNow;

        Raise(new SubscriptionWithdrawnDomainEvent(Id, OwnerId));

        return Result.Success();
    }

    public Result Cancel(DateTime utcNow)
    {
        if (Status is SubscriptionStatus.Withdrawn or SubscriptionStatus.Cancelled)
        {
            return Result.Failure(SubscriptionErrors.InvalidTransition(Id, Status, SubscriptionStatus.Cancelled));
        }

        Status = SubscriptionStatus.Cancelled;
        UpdatedAt = utcNow;

        Raise(new SubscriptionCancelledDomainEvent(Id, OwnerId));

        return Result.Success();
    }
}
