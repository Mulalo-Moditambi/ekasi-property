using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Subscriptions;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Properties.Relist;

internal sealed class RelistPropertyCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext,
    HybridCache cache,
    ISubscriptionAccessGuard subscriptionAccessGuard)
    : ICommandHandler<RelistPropertyCommand>
{
    public async Task<Result> Handle(RelistPropertyCommand command, CancellationToken cancellationToken)
    {
        Property? property = await context.Properties
            .SingleOrDefaultAsync(
                p => p.Id == command.PropertyId && p.OwnerId == userContext.UserId,
                cancellationToken);

        if (property is null)
        {
            return Result.Failure(PropertyErrors.NotFound(command.PropertyId));
        }

        Result accessResult = await subscriptionAccessGuard.EnsureCanListAsync(property.OwnerId, cancellationToken);

        if (accessResult.IsFailure)
        {
            return accessResult;
        }

        Result result = property.Relist(dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(PropertyCacheKeys.ById(property.Id), cancellationToken);

        return Result.Success();
    }
}
