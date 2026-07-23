namespace Infrastructure.Payments;

public sealed class PayFastOptions
{
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantKey { get; set; } = string.Empty;
    public string Passphrase { get; set; } = string.Empty;
    public string ProcessUrl { get; set; } = string.Empty;
    public string ValidateUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
}
