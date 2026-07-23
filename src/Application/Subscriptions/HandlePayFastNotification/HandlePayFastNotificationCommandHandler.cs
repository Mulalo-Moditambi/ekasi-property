using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Payments;
using Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Subscriptions.HandlePayFastNotification;

internal sealed class HandlePayFastNotificationCommandHandler(
    IApplicationDbContext context,
    IPaymentNotificationValidator notificationValidator,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<HandlePayFastNotificationCommand>
{
    public async Task<Result> Handle(HandlePayFastNotificationCommand command, CancellationToken cancellationToken)
    {
        bool isValid = await notificationValidator.IsValidAsync(command.Fields, command.Signature, cancellationToken);

        if (!isValid)
        {
            return Result.Failure(SubscriptionErrors.InvalidNotification("signature mismatch"));
        }

        if (!command.Fields.TryGetValue("pf_payment_id", out string? pfPaymentId) || string.IsNullOrWhiteSpace(pfPaymentId))
        {
            return Result.Failure(SubscriptionErrors.InvalidNotification("missing pf_payment_id"));
        }

        bool alreadyProcessed = await context.SubscriptionPayments
            .AnyAsync(p => p.PfPaymentId == pfPaymentId, cancellationToken);

        if (alreadyProcessed)
        {
            return Result.Success();
        }

        if (!command.Fields.TryGetValue("custom_str1", out string? subscriptionIdRaw) ||
            !Guid.TryParse(subscriptionIdRaw, out Guid subscriptionId))
        {
            return Result.Failure(SubscriptionErrors.InvalidNotification("missing or invalid custom_str1"));
        }

        Subscription? subscription = await context.Subscriptions
            .SingleOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure(SubscriptionErrors.NotFoundById(subscriptionId));
        }

        DateTime utcNow = dateTimeProvider.UtcNow;
        command.Fields.TryGetValue("payment_status", out string? paymentStatus);
        command.Fields.TryGetValue("token", out string? payFastToken);

        decimal amountGross = command.Fields.TryGetValue("amount_gross", out string? amountRaw) &&
            decimal.TryParse(amountRaw, out decimal parsedAmount)
            ? parsedAmount
            : 0m;

        context.SubscriptionPayments.Add(new SubscriptionPayment
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            PfPaymentId = pfPaymentId,
            PaymentStatus = paymentStatus ?? string.Empty,
            AmountGross = amountGross,
            ReceivedAt = utcNow
        });

        if (string.Equals(paymentStatus, "COMPLETE", StringComparison.OrdinalIgnoreCase))
        {
            subscription.RecordSuccessfulPayment(utcNow, utcNow.AddMonths(1), payFastToken);
        }
        else
        {
            subscription.EnterGracePeriod(utcNow);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
