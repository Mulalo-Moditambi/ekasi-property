using SharedKernel;

namespace Application.Abstractions.Subscriptions;

public interface ISubscriptionAccessGuard
{
    Task<Result> EnsureCanListAsync(Guid ownerId, CancellationToken cancellationToken);

    Task<Result> EnsureCanCreateListingAsync(Guid ownerId, DateTime utcNow, CancellationToken cancellationToken);
}
