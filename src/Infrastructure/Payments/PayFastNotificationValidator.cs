using Application.Abstractions.Payments;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments;

internal sealed class PayFastNotificationValidator(
    IOptions<PayFastOptions> options,
    PayFastSignatureService signatureService,
    IHttpClientFactory httpClientFactory,
    ILogger<PayFastNotificationValidator> logger)
    : IPaymentNotificationValidator
{
    private readonly PayFastOptions _options = options.Value;

    public async Task<bool> IsValidAsync(
        IReadOnlyDictionary<string, string> itnFields,
        string receivedSignature,
        CancellationToken cancellationToken)
    {
        string expectedSignature = signatureService.Sign(itnFields, _options.Passphrase);

        if (!string.Equals(expectedSignature, receivedSignature, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            HttpClient client = httpClientFactory.CreateClient("PayFast");

            using var content = new FormUrlEncodedContent(itnFields);

            HttpResponseMessage response = await client.PostAsync(_options.ValidateUrl, content, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            return body.Trim().Equals("VALID", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Defense-in-depth only: the local signature check above already passed.
            // Fail open on transport errors so a flaky remote validate call doesn't drop a legitimate payment.
            logger.LogWarning(ex, "PayFast remote ITN validation call failed; accepting based on local signature match");

            return true;
        }
    }
}
