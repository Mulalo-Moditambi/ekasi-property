using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Abstractions.Notifications;
using Domain.Notifications;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Infrastructure.Notifications;

internal sealed class WhatsAppCloudApiSender(IHttpClientFactory httpClientFactory, IOptions<WhatsAppOptions> options)
    : IWhatsAppSender
{
    private readonly WhatsAppOptions _options = options.Value;

    public async Task<Result> SendTemplateMessageAsync(
        string toPhoneE164,
        string templateName,
        IReadOnlyList<string> parameters,
        CancellationToken cancellationToken)
    {
        HttpClient client = httpClientFactory.CreateClient("WhatsApp");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        var payload = new
        {
            messaging_product = "whatsapp",
            to = toPhoneE164.TrimStart('+'),
            type = "template",
            template = new
            {
                name = templateName,
                language = new { code = "en" },
                components = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = parameters.Select(p => new { type = "text", text = p }).ToArray()
                    }
                }
            }
        };

        string requestUri = $"{_options.GraphApiBaseUrl}{_options.PhoneNumberId}/messages";

        HttpResponseMessage response = await client.PostAsJsonAsync(requestUri, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure(NotificationErrors.WhatsAppSendFailed(toPhoneE164));
        }

        return Result.Success();
    }
}
