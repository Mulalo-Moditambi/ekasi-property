namespace Infrastructure.Notifications;

public sealed class WhatsAppOptions
{
    public string AccessToken { get; set; } = string.Empty;
    public string PhoneNumberId { get; set; } = string.Empty;
    public string GraphApiBaseUrl { get; set; } = string.Empty;
    public string ReminderTemplateName { get; set; } = string.Empty;
}
