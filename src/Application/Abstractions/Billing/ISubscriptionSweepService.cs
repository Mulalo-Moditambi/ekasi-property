namespace Application.Abstractions.Billing;

public interface ISubscriptionSweepService
{
    Task RunOnceAsync(CancellationToken cancellationToken);
}
