using Application.Abstractions.Data;
using Application.Abstractions.Subscriptions;
using Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Subscriptions;

internal sealed class SubscriptionAccessGuard(IApplicationDbContext context) : ISubscriptionAccessGuard
{
    public async Task<Result> EnsureCanListAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        Subscription? subscription = await context.Subscriptions
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.OwnerId == ownerId, cancellationToken);

        if (subscription is null || !subscription.GrantsListingAccess())
        {
            return Result.Failure(SubscriptionErrors.PaymentRequired(ownerId));
        }

        return Result.Success();
    }

    public async Task<Result> EnsureCanCreateListingAsync(Guid ownerId, DateTime utcNow, CancellationToken cancellationToken)
    {
        Subscription? subscription = await context.Subscriptions
            .SingleOrDefaultAsync(s => s.OwnerId == ownerId, cancellationToken);

        if (subscription is null)
        {
            context.Subscriptions.Add(Subscription.StartTrial(ownerId, utcNow));

            return Result.Success();
        }

        if (!subscription.GrantsListingAccess())
        {
            return Result.Failure(SubscriptionErrors.PaymentRequired(ownerId));
        }

        return Result.Success();
    }
}
