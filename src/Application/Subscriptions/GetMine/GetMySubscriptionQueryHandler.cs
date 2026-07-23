using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Subscriptions.GetMine;

internal sealed class GetMySubscriptionQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetMySubscriptionQuery, SubscriptionResponse>
{
    public async Task<Result<SubscriptionResponse>> Handle(
        GetMySubscriptionQuery query, CancellationToken cancellationToken)
    {
        SubscriptionResponse? subscription = await context.Subscriptions
            .Where(s => s.OwnerId == userContext.UserId)
            .Select(s => new SubscriptionResponse
            {
                Id = s.Id,
                Status = s.Status,
                TrialEndsAt = s.TrialEndsAt,
                GracePeriodEndsAt = s.GracePeriodEndsAt,
                NextBillingDate = s.NextBillingDate
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<SubscriptionResponse>(SubscriptionErrors.NotFound(userContext.UserId));
        }

        return subscription;
    }
}
