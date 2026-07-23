using Application.Abstractions.Authentication;
using Application.Abstractions.Subscriptions;
using Application.Properties.Create;
using Application.UnitTests.Abstractions;
using Domain.Properties;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UnitTests.Properties;

public sealed class CreatePropertyCommandHandlerTests : BaseHandlerTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    private static CreatePropertyCommand Command => new()
    {
        OwnerId = OwnerId,
        Title = "Neat backroom with own entrance",
        Description = "A tidy backroom in Soweto, prepaid electricity included.",
        ListingType = ListingType.Rent,
        PropertyType = PropertyType.Backroom,
        Price = 1800m,
        Street = "123 Vilakazi Street",
        Township = "Orlando West",
        City = "Soweto",
        Province = "Gauteng",
        PostalCode = "1804",
        Bedrooms = 1,
        Bathrooms = 1,
        HasElectricity = true,
        WaterIncluded = true,
        HasOwnEntrance = true,
        HasParking = false
    };

    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_WhenOwnerIdDoesNotMatchContext()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(Guid.NewGuid());
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ISubscriptionAccessGuard accessGuard = Substitute.For<ISubscriptionAccessGuard>();

        var handler = new CreatePropertyCommandHandler(context, dateTimeProvider, userContext, accessGuard);

        // Act
        Result<Guid> result = await handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.Unauthorized());
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenOwnerDoesNotExist()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ISubscriptionAccessGuard accessGuard = Substitute.For<ISubscriptionAccessGuard>();

        var handler = new CreatePropertyCommandHandler(context, dateTimeProvider, userContext, accessGuard);

        // Act
        Result<Guid> result = await handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.NotFound(OwnerId));
    }

    [Fact]
    public async Task Handle_Should_PersistListedPropertyAndRaiseDomainEvent_WhenValid()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        context.Users.Add(new User
        {
            Id = OwnerId,
            Email = "owner@example.com",
            FirstName = "Test",
            LastName = "Owner",
            PasswordHash = "hash",
            PhoneNumber = "+27821234567"
        });
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        ISubscriptionAccessGuard accessGuard = Substitute.For<ISubscriptionAccessGuard>();
        accessGuard.EnsureCanCreateListingAsync(OwnerId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var handler = new CreatePropertyCommandHandler(context, dateTimeProvider, userContext, accessGuard);

        // Act
        Result<Guid> result = await handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Property property = await context.Properties.SingleAsync(p => p.Id == result.Value);
        property.OwnerId.ShouldBe(OwnerId);
        property.Title.ShouldBe("Neat backroom with own entrance");
        property.Status.ShouldBe(PropertyStatus.Listed);
        property.Address.Township.ShouldBe("Orlando West");
        property.DomainEvents.ShouldContain(domainEvent => domainEvent is PropertyListedDomainEvent);
    }

    [Fact]
    public async Task Handle_Should_ReturnPaymentRequired_WhenSubscriptionDoesNotGrantAccess()
    {
        // Arrange
        await using TestDbContext context = CreateDbContext();
        context.Users.Add(new User
        {
            Id = OwnerId,
            Email = "owner@example.com",
            FirstName = "Test",
            LastName = "Owner",
            PasswordHash = "hash",
            PhoneNumber = "+27821234567"
        });
        await context.SaveChangesAsync();

        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(OwnerId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ISubscriptionAccessGuard accessGuard = Substitute.For<ISubscriptionAccessGuard>();
        Error paymentRequired = Domain.Subscriptions.SubscriptionErrors.PaymentRequired(OwnerId);
        accessGuard.EnsureCanCreateListingAsync(OwnerId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(paymentRequired));

        var handler = new CreatePropertyCommandHandler(context, dateTimeProvider, userContext, accessGuard);

        // Act
        Result<Guid> result = await handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(paymentRequired);
    }
}
