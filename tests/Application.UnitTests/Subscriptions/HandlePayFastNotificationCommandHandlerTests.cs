using Application.Abstractions.Payments;
using Application.Subscriptions.HandlePayFastNotification;
using Application.UnitTests.Abstractions;
using Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Subscriptions;

public sealed class HandlePayFastNotificationCommandHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_Should_Fail_WhenSignatureIsInvalid()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        IPaymentNotificationValidator validator = Substitute.For<IPaymentNotificationValidator>();
        validator.IsValidAsync(Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();

        var handler = new HandlePayFastNotificationCommandHandler(context, validator, dateTimeProvider);
        var command = new HandlePayFastNotificationCommand(new Dictionary<string, string>(), "bad-signature");

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_ActivateSubscription_WhenPaymentComplete()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        IPaymentNotificationValidator validator = Substitute.For<IPaymentNotificationValidator>();
        validator.IsValidAsync(Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(UtcNow);

        var handler = new HandlePayFastNotificationCommandHandler(context, validator, dateTimeProvider);
        var fields = new Dictionary<string, string>
        {
            ["pf_payment_id"] = "pf-123",
            ["custom_str1"] = subscription.Id.ToString(),
            ["payment_status"] = "COMPLETE",
            ["token"] = "payfast-token"
        };
        var command = new HandlePayFastNotificationCommand(fields, "valid-signature");

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Subscription updated = await context.Subscriptions.SingleAsync(s => s.Id == subscription.Id);
        updated.Status.ShouldBe(SubscriptionStatus.Active);
        updated.PayFastToken.ShouldBe("payfast-token");

        SubscriptionPayment payment = await context.SubscriptionPayments.SingleAsync(p => p.PfPaymentId == "pf-123");
        payment.SubscriptionId.ShouldBe(subscription.Id);
    }

    [Fact]
    public async Task Handle_Should_EnterGracePeriod_WhenPaymentFailed()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        IPaymentNotificationValidator validator = Substitute.For<IPaymentNotificationValidator>();
        validator.IsValidAsync(Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(UtcNow);

        var handler = new HandlePayFastNotificationCommandHandler(context, validator, dateTimeProvider);
        var fields = new Dictionary<string, string>
        {
            ["pf_payment_id"] = "pf-456",
            ["custom_str1"] = subscription.Id.ToString(),
            ["payment_status"] = "FAILED"
        };
        var command = new HandlePayFastNotificationCommand(fields, "valid-signature");

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Subscription updated = await context.Subscriptions.SingleAsync(s => s.Id == subscription.Id);
        updated.Status.ShouldBe(SubscriptionStatus.GracePeriod);
    }

    [Fact]
    public async Task Handle_Should_BeIdempotent_WhenPfPaymentIdAlreadyProcessed()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        var subscription = Subscription.StartTrial(OwnerId, UtcNow);
        context.Subscriptions.Add(subscription);
        context.SubscriptionPayments.Add(new SubscriptionPayment
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            PfPaymentId = "pf-789",
            PaymentStatus = "COMPLETE",
            AmountGross = 25m,
            ReceivedAt = UtcNow
        });
        await context.SaveChangesAsync();

        IPaymentNotificationValidator validator = Substitute.For<IPaymentNotificationValidator>();
        validator.IsValidAsync(Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(UtcNow);

        var handler = new HandlePayFastNotificationCommandHandler(context, validator, dateTimeProvider);
        var fields = new Dictionary<string, string>
        {
            ["pf_payment_id"] = "pf-789",
            ["custom_str1"] = subscription.Id.ToString(),
            ["payment_status"] = "COMPLETE"
        };
        var command = new HandlePayFastNotificationCommand(fields, "valid-signature");

        // Act
        Result result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        int paymentCount = await context.SubscriptionPayments.CountAsync(p => p.PfPaymentId == "pf-789");
        paymentCount.ShouldBe(1);

        Subscription unchanged = await context.Subscriptions.SingleAsync(s => s.Id == subscription.Id);
        unchanged.Status.ShouldBe(SubscriptionStatus.Trialing);
    }
}
