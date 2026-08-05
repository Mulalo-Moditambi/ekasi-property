using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Storage;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Properties.DeleteImage;

internal sealed class DeletePropertyImageCommandHandler(
    IApplicationDbContext context,
    IFileStorage fileStorage,
    IUserContext userContext,
    HybridCache cache)
    : ICommandHandler<DeletePropertyImageCommand>
{
    public async Task<Result> Handle(DeletePropertyImageCommand command, CancellationToken cancellationToken)
    {
        bool ownsProperty = await context.Properties.AsNoTracking()
            .AnyAsync(p => p.Id == command.PropertyId && p.OwnerId == userContext.UserId, cancellationToken);

        if (!ownsProperty)
        {
            return Result.Failure(PropertyErrors.NotFound(command.PropertyId));
        }

        PropertyImage? image = await context.PropertyImages
            .SingleOrDefaultAsync(
                i => i.Id == command.ImageId && i.PropertyId == command.PropertyId,
                cancellationToken);

        if (image is null)
        {
            return Result.Failure(PropertyErrors.ImageNotFound(command.PropertyId, command.ImageId));
        }

        context.PropertyImages.Remove(image);

        await context.SaveChangesAsync(cancellationToken);

        await fileStorage.DeleteAsync(image.Url, cancellationToken);

        await cache.RemoveAsync(PropertyCacheKeys.ById(command.PropertyId), cancellationToken);

        return Result.Success();
    }
}
