namespace Application.Abstractions.Payments;

public interface IPaymentNotificationValidator
{
    Task<bool> IsValidAsync(
        IReadOnlyDictionary<string, string> itnFields,
        string receivedSignature,
        CancellationToken cancellationToken);
}
