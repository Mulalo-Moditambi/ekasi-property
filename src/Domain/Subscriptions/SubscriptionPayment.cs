using SharedKernel;

namespace Domain.Subscriptions;

public sealed class SubscriptionPayment : Entity
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public string PfPaymentId { get; set; }
    public string PaymentStatus { get; set; }
    public decimal AmountGross { get; set; }
    public DateTime ReceivedAt { get; set; }
}
