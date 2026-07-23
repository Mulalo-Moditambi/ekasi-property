using Application.Subscriptions;
using Application.UnitTests.Abstractions;
using Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Subscriptions;

public sealed class SubscriptionAccessGuardTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task EnsureCanListAsync_Should_ReturnPaymentRequired_WhenNoSubscriptionExists()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        var guard = new SubscriptionAccessGuard(context);

        // Act
        Result result = await guard.EnsureCanListAsync(OwnerId, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SubscriptionErrors.PaymentRequired(OwnerId));
    }

    [Theory]
    [InlineData(SubscriptionStatus.Trialing)]
    [InlineData(SubscriptionStatus.Active)]
    public async Task EnsureCanListAsync_Should_Succeed_WhenTrialingOrActive(SubscriptionStatus status)
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        context.Subscriptions.Add(CreateSubscription(status));
        await context.SaveChangesAsync();

        var guard = new SubscriptionAccessGuard(context);

        // Act
        Result result = await guard.EnsureCanListAsync(OwnerId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData(SubscriptionStatus.GracePeriod)]
    [InlineData(SubscriptionStatus.Withdrawn)]
    [InlineData(SubscriptionStatus.Cancelled)]
    public async Task EnsureCanListAsync_Should_ReturnPaymentRequired_WhenNotTrialingOrActive(SubscriptionStatus status)
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        context.Subscriptions.Add(CreateSubscription(status));
        await context.SaveChangesAsync();

        var guard = new SubscriptionAccessGuard(context);

        // Act
        Result result = await guard.EnsureCanListAsync(OwnerId, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SubscriptionErrors.PaymentRequired(OwnerId));
    }

    [Fact]
    public async Task EnsureCanCreateListingAsync_Should_StartTrialAndSucceed_WhenNoSubscriptionExists()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        var guard = new SubscriptionAccessGuard(context);

        // Act
        Result result = await guard.EnsureCanCreateListingAsync(OwnerId, UtcNow, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Subscription subscription = await context.Subscriptions.SingleAsync(s => s.OwnerId == OwnerId);
        subscription.Status.ShouldBe(SubscriptionStatus.Trialing);
        subscription.TrialStartedAt.ShouldBe(UtcNow);
    }

    [Theory]
    [InlineData(SubscriptionStatus.Trialing)]
    [InlineData(SubscriptionStatus.Active)]
    public async Task EnsureCanCreateListingAsync_Should_Succeed_WithoutCreatingAnotherSubscription_WhenTrialingOrActive(
        SubscriptionStatus status)
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        context.Subscriptions.Add(CreateSubscription(status));
        await context.SaveChangesAsync();

        var guard = new SubscriptionAccessGuard(context);

        // Act
        Result result = await guard.EnsureCanCreateListingAsync(OwnerId, UtcNow, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        (await context.Subscriptions.CountAsync(s => s.OwnerId == OwnerId)).ShouldBe(1);
    }

    [Theory]
    [InlineData(SubscriptionStatus.GracePeriod)]
    [InlineData(SubscriptionStatus.Withdrawn)]
    [InlineData(SubscriptionStatus.Cancelled)]
    public async Task EnsureCanCreateListingAsync_Should_ReturnPaymentRequired_WhenExistingSubscriptionLacksAccess(
        SubscriptionStatus status)
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        context.Subscriptions.Add(CreateSubscription(status));
        await context.SaveChangesAsync();

        var guard = new SubscriptionAccessGuard(context);

        // Act
        Result result = await guard.EnsureCanCreateListingAsync(OwnerId, UtcNow, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SubscriptionErrors.PaymentRequired(OwnerId));
    }

    private static Subscription CreateSubscription(SubscriptionStatus status)
    {
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        subscription.Status = status;

        return subscription;
    }
}
