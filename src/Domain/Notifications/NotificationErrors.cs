using SharedKernel;

namespace Domain.Notifications;

public static class NotificationErrors
{
    public static Error WhatsAppSendFailed(string phoneNumber) => Error.Failure(
        "Notifications.WhatsAppSendFailed",
        $"Failed to send a WhatsApp message to '{phoneNumber}'");
}
