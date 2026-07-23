using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Subscriptions;
using Domain.Properties;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Properties.Create;

internal sealed class CreatePropertyCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext,
    ISubscriptionAccessGuard subscriptionAccessGuard)
    : ICommandHandler<CreatePropertyCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePropertyCommand command, CancellationToken cancellationToken)
    {
        if (userContext.UserId != command.OwnerId)
        {
            return Result.Failure<Guid>(UserErrors.Unauthorized());
        }

        User? owner = await context.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == command.OwnerId, cancellationToken);

        if (owner is null)
        {
            return Result.Failure<Guid>(UserErrors.NotFound(command.OwnerId));
        }

        Result accessResult = await subscriptionAccessGuard.EnsureCanCreateListingAsync(
            owner.Id, dateTimeProvider.UtcNow, cancellationToken);

        if (accessResult.IsFailure)
        {
            return Result.Failure<Guid>(accessResult.Error);
        }

        var address = new Address(
            command.Street,
            command.Township,
            command.City,
            command.Province,
            command.PostalCode);

        var property = Property.Create(
            owner.Id,
            command.Title,
            command.Description,
            command.ListingType,
            command.PropertyType,
            command.Price,
            address,
            command.Bedrooms,
            command.Bathrooms,
            command.HasElectricity,
            command.WaterIncluded,
            command.HasOwnEntrance,
            command.HasParking,
            dateTimeProvider.UtcNow);

        context.Properties.Add(property);

        await context.SaveChangesAsync(cancellationToken);

        return property.Id;
    }
}
