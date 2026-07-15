using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Properties.Delete;

internal sealed class DeletePropertyCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    HybridCache cache)
    : ICommandHandler<DeletePropertyCommand>
{
    public async Task<Result> Handle(DeletePropertyCommand command, CancellationToken cancellationToken)
    {
        Property? property = await context.Properties
            .SingleOrDefaultAsync(
                p => p.Id == command.PropertyId && p.OwnerId == userContext.UserId,
                cancellationToken);

        if (property is null)
        {
            return Result.Failure(PropertyErrors.NotFound(command.PropertyId));
        }

        property.Raise(new PropertyDeletedDomainEvent(property.Id));

        context.Properties.Remove(property);

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(PropertyCacheKeys.ById(property.Id), cancellationToken);

        return Result.Success();
    }
}
