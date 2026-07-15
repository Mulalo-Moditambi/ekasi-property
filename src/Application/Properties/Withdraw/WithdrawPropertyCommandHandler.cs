using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Properties.Withdraw;

internal sealed class WithdrawPropertyCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext,
    HybridCache cache)
    : ICommandHandler<WithdrawPropertyCommand>
{
    public async Task<Result> Handle(WithdrawPropertyCommand command, CancellationToken cancellationToken)
    {
        Property? property = await context.Properties
            .SingleOrDefaultAsync(
                p => p.Id == command.PropertyId && p.OwnerId == userContext.UserId,
                cancellationToken);

        if (property is null)
        {
            return Result.Failure(PropertyErrors.NotFound(command.PropertyId));
        }

        Result result = property.Withdraw(dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(PropertyCacheKeys.ById(property.Id), cancellationToken);

        return Result.Success();
    }
}
