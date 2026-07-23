using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Payments;
using Domain.Subscriptions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Subscriptions.InitiateCheckout;

internal sealed class InitiateCheckoutCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IPaymentGateway paymentGateway)
    : ICommandHandler<InitiateCheckoutCommand, CheckoutResponse>
{
    public async Task<Result<CheckoutResponse>> Handle(
        InitiateCheckoutCommand command, CancellationToken cancellationToken)
    {
        User? owner = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (owner is null)
        {
            return Result.Failure<CheckoutResponse>(UserErrors.NotFound(userContext.UserId));
        }

        Subscription? subscription = await context.Subscriptions
            .SingleOrDefaultAsync(s => s.OwnerId == userContext.UserId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<CheckoutResponse>(SubscriptionErrors.NotFound(userContext.UserId));
        }

        PaymentCheckoutSession session = paymentGateway.CreateRecurringCheckout(new PaymentCheckoutRequest(
            subscription.Id,
            owner.Id,
            owner.Email,
            owner.FirstName,
            owner.LastName,
            Subscription.MonthlyFeeZar));

        subscription.PayFastMerchantPaymentId = session.MerchantPaymentId;

        await context.SaveChangesAsync(cancellationToken);

        return new CheckoutResponse(session.RedirectUrl);
    }
}
