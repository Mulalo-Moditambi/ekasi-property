using SharedKernel;

namespace Domain.Subscriptions;

public static class SubscriptionErrors
{
    public static Error NotFound(Guid ownerId) => Error.NotFound(
        "Subscriptions.NotFound",
        $"No subscription was found for owner with the Id = '{ownerId}'");

    public static Error NotFoundById(Guid subscriptionId) => Error.NotFound(
        "Subscriptions.NotFoundById",
        $"The subscription with the Id = '{subscriptionId}' was not found");

    public static Error InvalidNotification(string reason) => Error.Problem(
        "Subscriptions.InvalidNotification",
        $"The payment notification could not be processed: {reason}");

    public static Error PaymentRequired(Guid ownerId) => Error.Conflict(
        "Subscriptions.PaymentRequired",
        $"The owner with the Id = '{ownerId}' does not have an active subscription or trial");

    public static Error InvalidTransition(Guid subscriptionId, SubscriptionStatus from, SubscriptionStatus to) =>
        Error.Problem(
            "Subscriptions.InvalidTransition",
            $"The subscription with the Id = '{subscriptionId}' cannot transition from '{from}' to '{to}'");

    public static Error NotInGracePeriod(Guid subscriptionId) => Error.Problem(
        "Subscriptions.NotInGracePeriod",
        $"The subscription with the Id = '{subscriptionId}' is not in its grace period");

    public static Error InvalidReminderDay(int dayNumber) => Error.Problem(
        "Subscriptions.InvalidReminderDay",
        $"'{dayNumber}' is not a supported grace-period reminder day");

    public static Error Cancelled(Guid subscriptionId) => Error.Problem(
        "Subscriptions.Cancelled",
        $"The subscription with the Id = '{subscriptionId}' has been cancelled");
}
