using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Properties.Update;

internal sealed class UpdatePropertyCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext,
    HybridCache cache)
    : ICommandHandler<UpdatePropertyCommand>
{
    public async Task<Result> Handle(UpdatePropertyCommand command, CancellationToken cancellationToken)
    {
        Property? property = await context.Properties
            .SingleOrDefaultAsync(
                p => p.Id == command.PropertyId && p.OwnerId == userContext.UserId,
                cancellationToken);

        if (property is null)
        {
            return Result.Failure(PropertyErrors.NotFound(command.PropertyId));
        }

        if (property.Status == PropertyStatus.Sold)
        {
            return Result.Failure(PropertyErrors.AlreadySold(command.PropertyId));
        }

        property.Title = command.Title;
        property.Description = command.Description;
        property.Price = command.Price;
        property.Address = new Address(
            command.Street,
            command.Township,
            command.City,
            command.Province,
            command.PostalCode);
        property.Bedrooms = command.Bedrooms;
        property.Bathrooms = command.Bathrooms;
        property.HasElectricity = command.HasElectricity;
        property.WaterIncluded = command.WaterIncluded;
        property.HasOwnEntrance = command.HasOwnEntrance;
        property.HasParking = command.HasParking;
        property.UpdatedAt = dateTimeProvider.UtcNow;

        property.Raise(new PropertyUpdatedDomainEvent(property.Id));

        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(PropertyCacheKeys.ById(property.Id), cancellationToken);

        return Result.Success();
    }
}
