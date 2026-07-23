namespace Application.Abstractions.Payments;

public interface IPaymentGateway
{
    PaymentCheckoutSession CreateRecurringCheckout(PaymentCheckoutRequest request);
}

public sealed record PaymentCheckoutRequest(
    Guid SubscriptionId,
    Guid OwnerId,
    string Email,
    string FirstName,
    string LastName,
    decimal AmountZar);

public sealed record PaymentCheckoutSession(string RedirectUrl, string MerchantPaymentId);
