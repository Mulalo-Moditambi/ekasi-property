using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Subscriptions.Cancel;

internal sealed class CancelSubscriptionCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CancelSubscriptionCommand>
{
    public async Task<Result> Handle(CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        Subscription? subscription = await context.Subscriptions
            .SingleOrDefaultAsync(s => s.OwnerId == userContext.UserId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure(SubscriptionErrors.NotFound(userContext.UserId));
        }

        Result result = subscription.Cancel(dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
