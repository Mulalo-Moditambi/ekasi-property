using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Storage;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Properties.AddImages;

internal sealed class AddPropertyImagesCommandHandler(
    IApplicationDbContext context,
    IFileStorage fileStorage,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext,
    HybridCache cache)
    : ICommandHandler<AddPropertyImagesCommand, List<Guid>>
{
    private const int MaxImagesPerProperty = 10;

    public async Task<Result<List<Guid>>> Handle(AddPropertyImagesCommand command, CancellationToken cancellationToken)
    {
        Property? property = await context.Properties.AsNoTracking()
            .SingleOrDefaultAsync(
                p => p.Id == command.PropertyId && p.OwnerId == userContext.UserId,
                cancellationToken);

        if (property is null)
        {
            return Result.Failure<List<Guid>>(PropertyErrors.NotFound(command.PropertyId));
        }

        int existingCount = await context.PropertyImages
            .CountAsync(i => i.PropertyId == property.Id, cancellationToken);

        if (existingCount + command.Images.Count > MaxImagesPerProperty)
        {
            return Result.Failure<List<Guid>>(PropertyErrors.TooManyImages(property.Id, MaxImagesPerProperty));
        }

        var imageIds = new List<Guid>();
        int sortOrder = existingCount;

        foreach (ImageUpload upload in command.Images)
        {
            string extension = Path.GetExtension(upload.FileName);
            string url = await fileStorage.SaveAsync(upload.Content, extension, cancellationToken);

            var image = PropertyImage.Create(property.Id, url, sortOrder++, dateTimeProvider.UtcNow);

            context.PropertyImages.Add(image);
            imageIds.Add(image.Id);
        }

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(PropertyCacheKeys.ById(property.Id), cancellationToken);

        return imageIds;
    }
}
