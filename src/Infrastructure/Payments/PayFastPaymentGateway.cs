using System.Globalization;
using System.Net;
using Application.Abstractions.Payments;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments;

internal sealed class PayFastPaymentGateway(IOptions<PayFastOptions> options, PayFastSignatureService signatureService)
    : IPaymentGateway
{
    private readonly PayFastOptions _options = options.Value;

    public PaymentCheckoutSession CreateRecurringCheckout(PaymentCheckoutRequest request)
    {
        string merchantPaymentId = Guid.NewGuid().ToString("N");

        var fields = new List<KeyValuePair<string, string>>
        {
            new("merchant_id", _options.MerchantId),
            new("merchant_key", _options.MerchantKey),
            new("return_url", _options.ReturnUrl),
            new("cancel_url", _options.CancelUrl),
            new("notify_url", _options.NotifyUrl),
            new("m_payment_id", merchantPaymentId),
            new("amount", request.AmountZar.ToString("F2", CultureInfo.InvariantCulture)),
            new("item_name", "ekasi-property monthly listing subscription"),
            new("name_first", request.FirstName),
            new("name_last", request.LastName),
            new("email_address", request.Email),
            new("subscription_type", "1"),
            new("recurring_amount", request.AmountZar.ToString("F2", CultureInfo.InvariantCulture)),
            new("frequency", "3"),
            new("cycles", "0"),
            new("custom_str1", request.SubscriptionId.ToString())
        };

        string signature = signatureService.Sign(fields, _options.Passphrase);

        string queryString = string.Join(
            '&',
            fields
                .Where(f => !string.IsNullOrEmpty(f.Value))
                .Select(f => $"{f.Key}={WebUtility.UrlEncode(f.Value)}"));

        string redirectUrl = $"{_options.ProcessUrl}?{queryString}&signature={signature}";

        return new PaymentCheckoutSession(redirectUrl, merchantPaymentId);
    }
}
