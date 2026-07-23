using Domain.Subscriptions;
using SharedKernel;

namespace Application.UnitTests.Subscriptions;

public sealed class SubscriptionTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void StartTrial_Should_SetTrialingStatusAndRaiseDomainEvent()
    {
        // Act
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);

        // Assert
        subscription.Status.ShouldBe(SubscriptionStatus.Trialing);
        subscription.TrialStartedAt.ShouldBe(UtcNow);
        subscription.TrialEndsAt.ShouldBe(UtcNow.AddDays(14));
        subscription.DomainEvents.ShouldContain(e => e is SubscriptionTrialStartedDomainEvent);
    }

    [Theory]
    [InlineData(SubscriptionStatus.Trialing)]
    [InlineData(SubscriptionStatus.Active)]
    public void EnterGracePeriod_Should_Succeed_WhenTrialingOrActive(SubscriptionStatus status)
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        subscription.Status = status;

        // Act
        Result result = subscription.EnterGracePeriod(UtcNow);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        subscription.Status.ShouldBe(SubscriptionStatus.GracePeriod);
        subscription.GracePeriodStartedAt.ShouldBe(UtcNow);
        subscription.GracePeriodEndsAt.ShouldBe(UtcNow.AddDays(7));
        subscription.DomainEvents.ShouldContain(e => e is SubscriptionEnteredGracePeriodDomainEvent);
    }

    [Theory]
    [InlineData(SubscriptionStatus.GracePeriod)]
    [InlineData(SubscriptionStatus.Withdrawn)]
    [InlineData(SubscriptionStatus.Cancelled)]
    public void EnterGracePeriod_Should_Fail_WhenNotTrialingOrActive(SubscriptionStatus status)
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        subscription.Status = status;

        // Act
        Result result = subscription.EnterGracePeriod(UtcNow);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void RecordReminderSent_Should_Fail_WhenNotInGracePeriod()
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);

        // Act
        Result result = subscription.RecordReminderSent(1, UtcNow);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SubscriptionErrors.NotInGracePeriod(subscription.Id));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(7)]
    public void RecordReminderSent_Should_Fail_ForUnsupportedDayNumber(int dayNumber)
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        subscription.EnterGracePeriod(UtcNow);

        // Act
        Result result = subscription.RecordReminderSent(dayNumber, UtcNow);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void RecordReminderSent_Should_StampCorrectColumn_WhenInGracePeriod()
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        subscription.EnterGracePeriod(UtcNow);

        // Act
        Result result = subscription.RecordReminderSent(4, UtcNow);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        subscription.Day4ReminderSentAt.ShouldBe(UtcNow);
        subscription.Day1ReminderSentAt.ShouldBeNull();
        subscription.Day6ReminderSentAt.ShouldBeNull();
    }

    [Fact]
    public void RecordSuccessfulPayment_Should_ActivateAndClearGracePeriodFields()
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        subscription.EnterGracePeriod(UtcNow);
        DateTime nextBillingDate = UtcNow.AddMonths(1);

        // Act
        Result result = subscription.RecordSuccessfulPayment(UtcNow, nextBillingDate, "payfast-token");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        subscription.Status.ShouldBe(SubscriptionStatus.Active);
        subscription.LastPaymentAt.ShouldBe(UtcNow);
        subscription.NextBillingDate.ShouldBe(nextBillingDate);
        subscription.PayFastToken.ShouldBe("payfast-token");
        subscription.GracePeriodStartedAt.ShouldBeNull();
        subscription.GracePeriodEndsAt.ShouldBeNull();
        subscription.DomainEvents.ShouldContain(e => e is SubscriptionActivatedDomainEvent);
    }

    [Fact]
    public void RecordSuccessfulPayment_Should_Fail_WhenCancelled()
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        subscription.Cancel(UtcNow);

        // Act
        Result result = subscription.RecordSuccessfulPayment(UtcNow, UtcNow.AddMonths(1), null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SubscriptionErrors.Cancelled(subscription.Id));
    }

    [Fact]
    public void MarkWithdrawn_Should_Succeed_OnlyFromGracePeriod()
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        subscription.EnterGracePeriod(UtcNow);

        // Act
        Result result = subscription.MarkWithdrawn(UtcNow);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        subscription.Status.ShouldBe(SubscriptionStatus.Withdrawn);
        subscription.DomainEvents.ShouldContain(e => e is SubscriptionWithdrawnDomainEvent);
    }

    [Fact]
    public void MarkWithdrawn_Should_Fail_WhenNotInGracePeriod()
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);

        // Act
        Result result = subscription.MarkWithdrawn(UtcNow);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Theory]
    [InlineData(SubscriptionStatus.Withdrawn)]
    [InlineData(SubscriptionStatus.Cancelled)]
    public void Cancel_Should_Fail_WhenAlreadyWithdrawnOrCancelled(SubscriptionStatus status)
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        subscription.Status = status;

        // Act
        Result result = subscription.Cancel(UtcNow);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Theory]
    [InlineData(SubscriptionStatus.Trialing, true)]
    [InlineData(SubscriptionStatus.Active, true)]
    [InlineData(SubscriptionStatus.GracePeriod, false)]
    [InlineData(SubscriptionStatus.Withdrawn, false)]
    [InlineData(SubscriptionStatus.Cancelled, false)]
    public void GrantsListingAccess_Should_ReflectStatus(SubscriptionStatus status, bool expected)
    {
        // Arrange
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        subscription.Status = status;

        // Act & Assert
        subscription.GrantsListingAccess().ShouldBe(expected);
    }
}
