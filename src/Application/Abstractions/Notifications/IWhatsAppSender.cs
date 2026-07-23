using SharedKernel;

namespace Application.Abstractions.Notifications;

public interface IWhatsAppSender
{
    Task<Result> SendTemplateMessageAsync(
        string toPhoneE164,
        string templateName,
        IReadOnlyList<string> parameters,
        CancellationToken cancellationToken);
}
