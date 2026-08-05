using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Properties.ReorderImages;

internal sealed class ReorderPropertyImagesCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    HybridCache cache)
    : ICommandHandler<ReorderPropertyImagesCommand>
{
    public async Task<Result> Handle(ReorderPropertyImagesCommand command, CancellationToken cancellationToken)
    {
        bool ownsProperty = await context.Properties.AsNoTracking()
            .AnyAsync(p => p.Id == command.PropertyId && p.OwnerId == userContext.UserId, cancellationToken);

        if (!ownsProperty)
        {
            return Result.Failure(PropertyErrors.NotFound(command.PropertyId));
        }

        List<PropertyImage> images = await context.PropertyImages
            .Where(i => i.PropertyId == command.PropertyId)
            .ToListAsync(cancellationToken);

        bool sameImageSet = images.Count == command.ImageIds.Count &&
            images.Select(i => i.Id).OrderBy(id => id).SequenceEqual(command.ImageIds.OrderBy(id => id));

        if (!sameImageSet)
        {
            return Result.Failure(PropertyErrors.InvalidImageOrder(command.PropertyId));
        }

        for (int i = 0; i < command.ImageIds.Count; i++)
        {
            PropertyImage image = images.Single(existing => existing.Id == command.ImageIds[i]);
            image.SortOrder = i;
        }

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(PropertyCacheKeys.ById(command.PropertyId), cancellationToken);

        return Result.Success();
    }
}
